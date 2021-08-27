using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using MCMapExport.NBT.Tags;
using Microsoft.Toolkit.HighPerformance;

namespace MCMapExport.NBT {
    public partial class NBTReader : IDisposable {
        private readonly Stream _data;
        private readonly bool _owner;
        private readonly NBTReaderConfiguration _config;
        private TagType _current = TagType.TagEnd;
        

        private Func<string, ITag> Functions(TagType type) =>
            (type, _config.UseIntArrays, _config.UseLongArrays) switch {
                (TagType.TagEnd, _, _) => GetTagNull,
                (TagType.TagByte, _, _) => GetTagByte,
                (TagType.TagInt, _, _) => GetTagInt,
                (TagType.TagShort, _, _) => GetTagShort,
                (TagType.TagLong, _, _) => GetTagLong,
                (TagType.TagFloat, _, _) => GetTagFloat,
                (TagType.TagDouble, _, _) => GetTagDouble,
                (TagType.TagByteArray, _, _) => GetTagByteArray,
                (TagType.TagList, _, _) => GetTagList,
                (TagType.TagString, _, _) => GetTagString,
                (TagType.TagCompound, _, _) => GetTagCompound,
                (TagType.TagIntArray, true, _) => GetTagIntArray,
                (TagType.TagIntArray, false, _) => GetTagByteArray,
                (TagType.TagLongArray, true, _) => GetTagLongArray,
                (TagType.TagLongArray, false, _) => GetTagByteArray,
                _ => throw new ArgumentOutOfRangeException()
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

        public NBTReader(Stream stream, bool owner, NBTReaderConfiguration config = null) : this(config) {
            _data = stream;
            _owner = owner;
        }


        public ITag GetTag() {
            return GetTag(out _);
        }
        private ITag GetTag(out string name) {
            name = "";
            var byteType = ReadByte();
            // Reached the end of the stream, return null
            if (byteType == -1) {
                return null;
            }
            _current = (TagType)byteType;
            if (!_current.IsDefined()) // Value not defined in the list of known tags, something went wrong
                throw new Exception("Invalid NBT file, some of the tags being used are not defined.");

            // If the tag is not a TAG_end, it has a name
            if (_current != TagType.TagEnd) name = GetTagName();

            var tag = Functions(_current)(name);

            return tag;
        }

        private EndTag GetTagNull(string name) {
            return new EndTag();
        }

        private ByteTag GetTagByte(string name) {
            var payload = ReadByte();
            return new ByteTag {
                Payload = payload,
            };
        }

        private ShortTag GetTagShort(string name) {
            return new ShortTag {
                Payload = ReadShort(),
            };
        }
        
        private IntTag GetTagInt(string name) {
            return new IntTag {
                Payload = ReadInt(),
            };
        }

        private LongTag GetTagLong(string name) {
            return new LongTag {
                Payload = ReadLong(),
            };
        }

        private FloatTag GetTagFloat(string name) {
            return new FloatTag {
                Payload = ReadFloat(),
            };
        }

        private DoubleTag GetTagDouble(string name) {
            return new DoubleTag {
                Payload = ReadDouble(),
            };
        }


        private StringTag GetTagString(string name) {
            var tagSize = GetTagShort("");
            if (tagSize == null) return null;

            var payload = GetStringOfLength(tagSize.Payload);
            return new StringTag {
                Payload = payload,
            };
        }

        private ListTag GetTagList(string name) {
            var byteType = ReadByte();
            _current = (TagType)byteType;
            var type = _current;
            var tagSize = ReadInt();

            var payload = new ITag[tagSize];
            for (var i = 0; i < tagSize; i++) {
                var func = Functions(type);
                var tag = func("");
                payload[i] = tag;
            }

            return new ListTag {
                Name = name,
                Payload = payload,
                ListType = type,
            };
        }

        private CompoundTag GetTagCompound(string name) {
            return new CompoundTag {
                Name = name,
                Payload = GetArrayOfTags(),
            };
        }

        private ByteArrayTag GetTagByteArray(string name) {
            var tagSize = ReadInt();
            if (_current == TagType.TagIntArray) {
                tagSize *= 4;
            } else if (_current == TagType.TagLongArray) {
                tagSize *= 8;
            }

            var payload = new ByteTag[tagSize];

            for (var i = 0; i < tagSize; i++) {
                payload[i] = GetTagByte("");
            }

            return new ByteArrayTag {
                Name = name,
                Payload = payload,
            };
        }

        private IntArrayTag GetTagIntArray(string name) {
            var tagSize = ReadInt();

            var payload = new IntTag[tagSize];
            for (var i = 0; i < tagSize; i++) {
                payload[i] = GetTagInt("");
            }

            return new IntArrayTag {
                Name = name,
                Payload = payload,
            };
        }

        private LongArrayTag GetTagLongArray(string name) {
            var tagSize = ReadInt();

            var payload = new LongTag[tagSize];
            for (var i = 0; i < tagSize; i++) {
                payload[i] = GetTagLong("");
            }

            return new LongArrayTag() {
                Name = name,
                Payload = payload,
            };
        }

        private IDictionary<string, ITag> GetArrayOfTags() {
            var tags = new Dictionary<string, ITag>();
            var nextTag = GetTag(out var name);
            while (nextTag != null && nextTag is not EndTag) {
                tags.Add(name, nextTag);
                nextTag = GetTag(out name);
            }

            return tags;
        }

        private string GetTagName() {
            var name = GetStringOfLength(ReadShort());
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

        
        public void Dispose() {
            _data?.Dispose();
        }
    }
}