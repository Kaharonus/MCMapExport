using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace MCMapExport.Reader.Models {
    public class RawChunk {
        private byte[] _decompressed = null;

        public CompressionType Type { get; init; }
        public IEnumerable<byte> Data { get; init; }
        public uint Length { get; init; }
        public byte[] DecompressedData => Decompress();

        private byte[] Decompress() {
            if (_decompressed is not null) {
                return _decompressed;
            }

            using var compressed = new MemoryStream(Data.ToArray());
            using var output = new MemoryStream();
            switch (Type) {
                case CompressionType.GZip:
                    GZip.Decompress(compressed, output, false);
                    break;
                case CompressionType.Zlib: {
                    using var inputStream = new InflaterInputStream(compressed);
                    inputStream.CopyTo(output);
                    output.Position = 0;
                }
                    break;
                case CompressionType.Uncompressed:
                    _decompressed = Data.ToArray();
                    //BE CAREFUL HERE
                    return _decompressed;
                default:
                    throw new ArgumentOutOfRangeException();
            }
 
            _decompressed = new byte[output.Length];
            output.Read(_decompressed, 0, (int)output.Length);
            return _decompressed;
        }
    }
}