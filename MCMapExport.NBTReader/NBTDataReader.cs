using System;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using MCMapExport.NBT.Tags;
using Microsoft.Toolkit.HighPerformance;

namespace MCMapExport.NBT {
    public abstract class NBTDataReader : IDisposable {
        private readonly bool _owner;
        private readonly Stream _data;
        protected readonly NBTReaderConfiguration Config;

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

        private NBTDataReader(NBTReaderConfiguration config) {
            _data = new MemoryStream();
            Config = config ?? new NBTReaderConfiguration();
        }

        public NBTDataReader(ReadOnlyMemory<byte> bytes, CompressionType type, NBTReaderConfiguration config = null) :
            this(config) {
            using var compressed = bytes.AsStream();
            SetupStream(compressed, type);
            _owner = true;
        }

        public NBTDataReader(byte[] bytes, CompressionType type, NBTReaderConfiguration config = null) : this(config) {
            using var compressed = new MemoryStream(bytes);
            SetupStream(compressed, type);
            _owner = true;
        }

        public NBTDataReader(Stream stream, bool owner, NBTReaderConfiguration config = null) : this(config) {
            _data = stream;
            _owner = owner;
        }


        protected unsafe float ReadFloat() {
            var value = ReadInt();
            return *(float*)&value;
        }

        protected unsafe double ReadDouble() {
            var value = ReadLong();
            return *(double*)&value;
        }

        protected int ReadInt() {
            var b1 = ReadByte();
            var b2 = ReadByte();
            var b3 = ReadByte();
            var b4 = ReadByte();
            if (BitConverter.IsLittleEndian) {
                return (b1 << 24) + (b2 << 16) + (b3 << 8) + b4;
            }

            return (b4 << 24) + (b3 << 16) + (b2 << 8) + b1;
        }

        protected short ReadShort() {
            var b1 = ReadByte();
            var b2 = ReadByte();
            if (BitConverter.IsLittleEndian) {
                return (short)((b1 << 8) + b2);
            }

            return (short)((b2 << 8) + b1);
        }

        protected long ReadLong() {
            var b1 = ReadByte();
            var b2 = ReadByte();
            var b3 = ReadByte();
            var b4 = ReadByte();
            var b5 = ReadByte();
            var b6 = ReadByte();
            var b7 = ReadByte();
            var b8 = ReadByte();
            if (BitConverter.IsLittleEndian) {
                return ((long)((b1 << 24) + (b2 << 16) + (b3 << 8) + b4) << 32) +
                       ((b5 << 24) + (b6 << 16) + (b7 << 8) + b8);
            }

            return ((long)((b8 << 24) + (b7 << 16) + (b6 << 8) + b5) << 32) +
                   ((b4 << 24) + (b3 << 16) + (b2 << 8) + b1);
        }

        protected byte ReadByte() {
            return (byte)_data.ReadByte();
        }

        protected void ReadByte(ref byte b) {
            b = (byte)_data.ReadByte();
        }

        protected void ReadData(byte[] data) {
            _data.Read(data, 0, data.Length);
        }

        public void Dispose() {
            _data?.Dispose();
        }

        public TagType GetNextTagType() {
            return (TagType)ReadByte();
        }

        protected string ReadString() {
            var nameLength = (ushort)ReadShort();
            var nameBytes = new byte[nameLength];
            ReadData(nameBytes);
            var name = Encoding.UTF8.GetString(nameBytes);
            return name;
        }

        protected string GetTagName() {
            return GetStringOfLength(ReadShort());
        }

        protected string GetStringOfLength(short nameLength) {
            var length = (ushort)nameLength;
            var nameBytes = new byte[length];
            ReadData(nameBytes);
            var name = Encoding.UTF8.GetString(nameBytes);
            return name;
        }


        protected void SkipBytes(int count) {
            _data.Position += count;
        }
    }
}