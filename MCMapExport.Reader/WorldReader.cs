using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MCMapExport.Common.Models;
using MCMapExport.Reader.Models;

namespace MCMapExport.Reader {
    public class WorldReader {
        private const int BlockSize = 4096;

        private readonly string _directory;
        private byte[] _data;

        public static WorldReader? Open(string path, out string error) {
            error = "";
            if (!Directory.Exists(path)) {
                error = $"Directory {path} does not exist";
                return null;
            }

            var dirs = Directory.GetDirectories(path);
            var subDirs = new[] {"DIM-1", "DIM1", "region"};
            if (!dirs.Select(Path.GetFileName).ContainsAll(subDirs)) {
                error = $"Directory {path} does not contain all required subdirectories";
                return null;
            }

            var files = Directory.GetFiles(path);
            if (!files.Select(Path.GetFileName).Contains("level.dat")) {
                error = $"File {path}/file.dat not found";
                return null;
            }

            return new WorldReader(path);
        }


        private WorldReader(string directory) {
            _directory = directory;
        }

        private RawChunk ReadRawChunk(Header header) {
            var offset = (int) (BlockSize * header.Offset);
            var count = (_data[offset] << 24) | _data[offset + 1] << 16 | _data[offset + 2] << 8 | _data[offset + 3];
            return new RawChunk {
                Length = (uint) count,
                Type = (CompressionType) _data[offset + 4],
                Data = _data[(offset + 5).. (offset + 5 + count - 1)]
            };
        }

        public Map Read() {
            var files = Directory.GetFiles(_directory + "/region/").OrderBy(x => x);

            foreach (var filePath in files) {
                using var file = new BinaryReader(File.OpenRead(filePath));
                var headerData = new byte[8192];
                _data = new byte[file.BaseStream.Length - 8192];
                file.Read(headerData, 0, 8192);
                file.Read(_data, 0, (int) file.BaseStream.Length - 8192);

                var headers = HeaderReader.Parse(headerData);
                var chunks = new List<RawChunk>();
                foreach (var header in headers) {
                    var raw = ReadRawChunk(header);
                    chunks.Add(raw);
                }

                var max = chunks.Max(x => x.DecompressedData.Length);
                var maxChunk = chunks.First(x => x.DecompressedData.Length == max);
                ChunkReader.ReadChunk(maxChunk);
            }

            return default;
        }
    }
}