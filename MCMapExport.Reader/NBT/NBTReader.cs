using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace MCMapExport.Reader.NBT {
    public static class NBTReader {
        private static readonly Dictionary<TagType, Func<Stream, string, long, ITag>> TagFuncDict = new() {
                {TagType.TagEnd, GetTagNull},
                {TagType.TagByte, GetTagByte},
                {TagType.TagShort, GetTagShort},
                {TagType.TagInt, GetTagInt},
                {TagType.TagLong, GetTagLong},
                {TagType.TagFloat, GetTagFloat},
                {TagType.TagDouble, GetTagDouble},
                {TagType.TagByteArray, GetTagByteArray},
                {TagType.TagString, GetTagString},
                {TagType.TagList, GetTagList},
                {TagType.TagCompound, GetTagCompound},
                {TagType.TagIntArray, GetTagIntArray},
                {TagType.TagLongArray, GetTagLongArray}
            };

        public static ITag GetTag(Stream stream) {
            return GetTag(stream, 0);
        }
        
        /// <summary>
        /// Get an NBT tag, recursively loading each contained tag
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private static ITag GetTag(Stream stream, long depth) {
            var byteType = stream.ReadByte();
            // Reached the end of the stream, return null
            if (byteType == -1) {
                return null;
            }

            var type = (TagType) byteType;
            if (!Enum.IsDefined(typeof(TagType), type)) {
                // Value not defined in the list of known tags, something went wrong
                throw new Exception("Invalid NBT file, some of the tags being used are not defined.");
            }

            // If the tag is not a TAG_end, it has a name
            string name = null;
            if (type != TagType.TagEnd) {
                name = GetTagName(stream);
            }


            ITag tag = null;
            if (TagFuncDict.ContainsKey(type)) {
                tag = TagFuncDict[type](stream, name, depth);
            }
            
            return tag;
        }

        private static TagEnd GetTagNull(Stream stream, string name, long depth) {
            return new TagEnd() {
                Name = name,
                Depth = depth
            };
        }

        private static TagByte GetTagByte(Stream stream, string name, long depth) {
            var payloadBytes = new byte[1];
            ReadBytes(stream, payloadBytes);
            var payload = (sbyte) payloadBytes[0];

            return new TagByte() {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }

        private static TagShort GetTagShort(Stream stream, string name, long depth) {
            var payloadBytes = new byte[2];
            ReadBytes(stream, payloadBytes);
            var payload = BitConverter.ToInt16(payloadBytes, 0);

            return new TagShort() {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }

        private static TagInt GetTagInt(Stream stream, string name, long depth) {
            var payloadBytes = new byte[4];
            ReadBytes(stream, payloadBytes);
            var payload = BitConverter.ToInt32(payloadBytes, 0);

            return new TagInt() {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }

        private static TagLong GetTagLong(Stream stream, string name, long depth) {
            var payloadBytes = new byte[8];
            ReadBytes(stream, payloadBytes);
            var payload = BitConverter.ToInt64(payloadBytes, 0);

            return new TagLong() {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }

        private static TagFloat GetTagFloat(Stream stream, string name, long depth) {
            var payloadBytes = new byte[4];
            ReadBytes(stream, payloadBytes);
            var payload = BitConverter.ToSingle(payloadBytes, 0);

            return new TagFloat() {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }

        private static TagDouble GetTagDouble(Stream stream, string name, long depth) {
            var payloadBytes = new byte[8];
            ReadBytes(stream, payloadBytes);
            var payload = BitConverter.ToDouble(payloadBytes, 0);

            return new TagDouble() {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }

        private static TagByteArray GetTagByteArray(Stream stream, string name, long depth) {
            var tagSize = GetTagInt(stream, "_length", depth + 1);
            var payload = new List<TagByte>();

            for (var i = 0; i < tagSize.Payload; i++) {
                TagByte tag = GetTagByte(stream, $"_{i}", depth + 1);
                payload.Add(tag);
            }

            return new TagByteArray() {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }

        private static TagString GetTagString(Stream stream, string name, long depth) {
            var tagSize = GetTagShort(stream, "_length", depth + 1);
            if (tagSize == null) {
                return null;
            }

            var payload = GetStringOfLength(stream, tagSize.Payload);
            return new TagString() {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }

        private static TagList<ITag> GetTagList(Stream stream, string name, long depth) {
            var byteType = GetTagByte(stream, "_type", depth + 1);
            var type = (TagType) byteType.Payload;

            var tagSize = GetTagInt(stream, "_length", depth + 1);

            var payload = new List<ITag>();
            for (var i = 0; i < tagSize.Payload; i++) {
                var func = TagFuncDict[type];
                ITag tag = func(stream, $"_{i}", depth + 1);
                payload.Add(tag);
            }

            return new TagList<ITag>() {
                Name = name,
                Payload = payload,
                ListType = type,
                Depth = depth
            };
        }

        private static TagCompound GetTagCompound(Stream stream, string name, long depth) {
            return new TagCompound() {
                Name = name,
                Payload = GetArrayOfTags(stream, depth),
                Depth = depth
            };
        }

        private static TagIntArray GetTagIntArray(Stream stream, string name, long depth) {
            var tagSize = GetTagInt(stream, "_length", depth + 1);

            var payload = new List<TagInt>();
            for (var i = 0; i < tagSize.Payload; i++) {
                var tag = GetTagInt(stream, $"_{i}", depth + 1);
                payload.Add(tag);
            }

            return new TagIntArray() {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }

        private static TagLongArray GetTagLongArray(Stream stream, string name, long depth) {
            var tagSize = GetTagInt(stream, "_length", depth + 1);

            var payload = new List<TagLong>();
            for (var i = 0; i < tagSize.Payload; i++) {
                var tag = GetTagLong(stream, $"_{i}", depth + 1);
                payload.Add(tag);
            }

            return new TagLongArray() {
                Name = name,
                Payload = payload,
                Depth = depth
            };
        }

        /// <summary>
        /// Gets an array of tags until it reaches an end tag
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        private static IEnumerable<ITag> GetArrayOfTags(Stream stream, long depth) {
            var tags = new List<ITag>();
            var nextTag = GetTag(stream, depth + 1);
            while (nextTag != null && nextTag.Type != TagType.TagEnd) {
                tags.Add(nextTag);
                nextTag = GetTag(stream, depth + 1);
            }

            return tags;
        }

        /// <summary>
        /// Get the name of the tag (2 Byte specifying the name length in bytes, followed by a utf-8 encoded string)
        /// </summary>
        /// <param name="stream">The GZip decompressed filestream</param>
        /// <returns></returns>
        private static string GetTagName(Stream stream) {
            var bytes = new byte[2];
            ReadBytes(stream, bytes);

            var nameLength = BitConverter.ToUInt16(bytes, 0);
            var name = GetStringOfLength(stream, nameLength);
            return name;
        }

        /// <summary>
        /// Get a string of a given length from the stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="nameLength"></param>
        /// <returns></returns>
        private static string GetStringOfLength(Stream stream, ushort nameLength) {
            var nameBytes = new byte[nameLength];
            stream.Read(nameBytes, 0, nameLength);
            var name = Encoding.UTF8.GetString(nameBytes);
            return name;
        }

        /// <summary>
        /// Get a string of a given length from the stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="nameLength"></param>
        /// <returns></returns>
        private static string GetStringOfLength(Stream stream, short nameLength) {
            var length = Convert.ToUInt16(nameLength);
            return GetStringOfLength(stream, length);
        }

        /// <summary>
        /// Read as many bytes as needed to fill the array, with the correct endianness
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bytes"></param>
        private static void ReadBytes(Stream stream, byte[] bytes) {
            stream.Read(bytes, 0, bytes.Length);
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }
        }
    }
}