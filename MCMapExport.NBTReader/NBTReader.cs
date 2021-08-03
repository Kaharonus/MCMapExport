using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using MCMapExport.NBT.Tags;
using Microsoft.Toolkit.HighPerformance;

namespace MCMapExport.NBT {
    public class NBTReader : IDisposable {
        private readonly Stream _data;
        private readonly bool _owner;
        private readonly NBTReaderConfiguration _config;
        private TagType _current = TagType.TagEnd;

        private Func<string, long, ITag> Functions(TagType type) =>
            (type, _config.UseIntArrays, _config.UseLongArrays) switch {
                (TagType.TagEnd, _, _) => GetTagNull,
                (TagType.TagByte, _, _) => GetTagByte,
                (TagType.TagShort, _, _) => GetTagShort,
                (TagType.TagInt, _, _) => GetTagInt,
                (TagType.TagLong, _, _) => GetTagLong,
                (TagType.TagFloat, _, _) => GetTagFloat,
                (TagType.TagDouble, _, _) => GetTagDouble,
                (TagType.TagByteArray, _, _) => GetTagByteArray,
                (TagType.TagString, _, _) => GetTagString,
                (TagType.TagList, _, _) => GetTagList,
                (TagType.TagCompound, _, _) => GetTagCompound,
                (TagType.TagIntArray, true, _) => GetTagIntArray,
                (TagType.TagIntArray, false, _) => GetTagByteArray,
                (TagType.TagLongArray, _, true) => GetTagLongArray,
                (TagType.TagLongArray, _, false) => GetTagByteArray,
                (_) => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

        private NBTReader(NBTReaderConfiguration config) {
            _data = new MemoryStream();
            _config = config ?? new NBTReaderConfiguration();
        }

        private void SetupStream(Stream compressed, CompressionType type) {
            switch (type) {
                case CompressionType.GZip:
                    GZip.Decompress(compressed, _data, false);
                    break;
                case CompressionType.Zlib: {
                    using var output = new InflaterInputStream(compressed);
                    output.CopyTo(_data);
                }
                    break;
                case CompressionType.Uncompressed:
                    compressed.CopyTo(_data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            _data.Position = 0;

        }

        public NBTReader(ReadOnlyMemory<byte> bytes, CompressionType type, NBTReaderConfiguration config = null) :
            this(config) {
            using var compressed = bytes.AsStream();
            SetupStream(compressed, type);
            _owner = true;
        }
        
        public NBTReader(byte[] bytes, CompressionType type, NBTReaderConfiguration config = null) : this(config) {
            using var compressed = new MemoryStream(bytes);
            SetupStream(compressed, type);
            _owner = true;
        }

        public NBTReader(Stream stream, bool owner ,NBTReaderConfiguration config = null) : this(config) {
            _data = stream;
            _owner = owner;
        }


        public ITag GetTag() {
            return GetTag(0);
        }

        private ITag GetTag(long depth) {
            var byteType = _data.ReadByte();
            // Reached the end of the stream, return null
            if (byteType == -1) return null;

            _current = (TagType) byteType;
            if (!Enum.IsDefined(typeof(TagType),
                _current)) // Value not defined in the list of known tags, something went wrong
                throw new Exception("Invalid NBT file, some of the tags being used are not defined.");

            // If the tag is not a TAG_end, it has a name
            string name = null;
            if (_current != TagType.TagEnd) name = GetTagName();
            
            var tag = Functions(_current)(name, depth);

            return tag;
        }

        private EndTag GetTagNull(string name, long depth) {
            return new() {
                Name = name,
                Depth = depth
            };
        }

        private ByteTag GetTagByte(string name, long depth) {
            var payloadBytes = new byte[1];
            ReadBytes(payloadBytes);
            var payload = (sbyte) payloadBytes[0];

            return new ByteTag {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }

        private ShortTag GetTagShort(string name, long depth) {
            var payloadBytes = new byte[2];
            ReadBytes(payloadBytes);
            var payload = BitConverter.ToInt16(payloadBytes, 0);

            return new ShortTag {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }

        private IntTag GetTagInt(string name, long depth) {
            var payloadBytes = new byte[4];
            ReadBytes(payloadBytes);
            var payload = BitConverter.ToInt32(payloadBytes, 0);

            return new IntTag {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }

        private LongTag GetTagLong(string name, long depth) {
            var payloadBytes = new byte[8];
            ReadBytes(payloadBytes);
            var payload = BitConverter.ToInt64(payloadBytes, 0);

            return new LongTag {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }

        private FloatTag GetTagFloat(string name, long depth) {
            var payloadBytes = new byte[4];
            ReadBytes(payloadBytes);
            var payload = BitConverter.ToSingle(payloadBytes, 0);

            return new FloatTag {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }

        private DoubleTag GetTagDouble(string name, long depth) {
            var payloadBytes = new byte[8];
            ReadBytes(payloadBytes);
            var payload = BitConverter.ToDouble(payloadBytes, 0);

            return new DoubleTag {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }



        private StringTag GetTagString(string name, long depth) {
            var tagSize = GetTagShort("_length", depth + 1);
            if (tagSize == null) return null;

            var payload = GetStringOfLength(tagSize.Payload);
            return new StringTag {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }

        private ListTag GetTagList(string name, long depth) {
            var byteType = GetTagByte("_type", depth + 1);
            _current = (TagType) byteType.Payload;
            var type = _current;
            var tagSize = GetTagInt("_length", depth + 1);

            var payload = new List<ITag>();
            for (var i = 0; i < tagSize.Payload; i++) {
                var func = Functions(type);
                var tag = func($"_{i}", depth + 1);
                payload.Add(tag);
            }

            return new ListTag {
                Name = name,
                Payload = payload,
                ListType = type,
                Depth = depth
            };
        }

        private CompoundTag GetTagCompound(string name, long depth) {
            return new() {
                Name = name,
                Payload = GetArrayOfTags(depth).ToDictionary(x => x.Name, x => x),
                Depth = depth
            };
        }
        
        private ByteArrayTag GetTagByteArray(string name, long depth) {
            var tagSize = GetTagInt("_length", depth + 1).Payload;
            if (_current == TagType.TagIntArray) {
                tagSize *= 4;
            } else if (_current == TagType.TagLongArray) {
                tagSize *= 8;
            }
            var payload = new List<ByteTag>();
            
            for (var i = 0; i < tagSize; i++) {
                var tag = GetTagByte($"_{i}", depth + 1);
                payload.Add(tag);
            }

            return new ByteArrayTag {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }

        private IntArrayTag GetTagIntArray(string name, long depth) {
            var tagSize = GetTagInt("_length", depth + 1);

            var payload = new List<IntTag>();
            for (var i = 0; i < tagSize.Payload; i++) {
                var tag = GetTagInt($"_{i}", depth + 1);
                payload.Add(tag);
            }

            return new IntArrayTag {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }

        private LongArrayTag GetTagLongArray(string name, long depth) {
            var tagSize = GetTagInt("_length", depth + 1);

            var payload = new List<LongTag>();
            for (var i = 0; i < tagSize.Payload; i++) {
                var tag = GetTagLong($"_{i}", depth + 1);
                payload.Add(tag);
            }

            return new LongArrayTag() {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }

        private IEnumerable<ITag> GetArrayOfTags(long depth) {
            var tags = new List<ITag>();
            var nextTag = GetTag(depth + 1);
            while (nextTag != null && nextTag.Type != TagType.TagEnd) {
                tags.Add(nextTag);
                nextTag = GetTag(depth + 1);
            }

            return tags;
        }

        private string GetTagName() {
            var bytes = new byte[2];
            ReadBytes(bytes);

            var nameLength = BitConverter.ToUInt16(bytes, 0);
            var name = GetStringOfLength(nameLength);
            return name;
        }

        private string GetStringOfLength(ushort nameLength) {
            var nameBytes = new byte[nameLength];
            _data.Read(nameBytes, 0, nameLength);
            var name = Encoding.UTF8.GetString(nameBytes);
            return name;
        }

        private string GetStringOfLength(short nameLength) {
            var length = Convert.ToUInt16(nameLength);
            return GetStringOfLength(length);
        }

        private void ReadBytes(byte[] bytes) {
            _data.Read(bytes, 0, bytes.Length);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        }

        public void Dispose() {
            _data?.Dispose();
        }
    }
}