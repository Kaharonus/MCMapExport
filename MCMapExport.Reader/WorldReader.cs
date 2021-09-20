using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MCMapExport.Common;
using MCMapExport.Common.Enums;
using MCMapExport.Common.Models;
using MCMapExport.NBT;
using MCMapExport.NBT.Tags;
using MCMapExport.Reader.Models;

namespace MCMapExport.Reader {
    public class WorldReader {
        public static WorldReader? Create(string path, out string error) {
            error = null;
            if (!Directory.Exists(path)) {
                error = $"Directory {path} does not exist";
                return null;
            }

            var dirs = Directory.GetDirectories(path);
            var subDirs = new[] { "DIM-1", "DIM1", "region" };
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

        public string SavePath { get; }

        public List<(int x, int y)> Regions { get; set; } = new();

        private ConcurrentDictionary<(int x, int y), (List<Header> headers, byte[] data)> RegionData { get; set; } =
            new();

        private ConcurrentDictionary<(int x, int y), Chunk> ChunkData { get; set; } = new();
        private ConcurrentDictionary<(int x, int y), string> Files { get; } = new();

        private readonly NBTReaderConfiguration _readerConfiguration = new() {
            UseIntArrays = false,
            UseLongArrays = false
        };

        private WorldReader(string path) {
            SavePath = path;
            var files = Directory.GetFiles(SavePath + "/region/").OrderBy(x => x);
            var names = files.Select(
                x => (x, Regex.Match(x, @"-?[0-9]+\.-?[0-9]+\.mca$").Value.Split("."))
            );
            foreach (var (s, parts) in names) {
                var x = int.Parse(parts[0]);
                var z = int.Parse(parts[1]);
                Files.TryAdd((x, z), s);
            }

            var sw = Stopwatch.StartNew();
            GenerateAvailable();
            sw.Stop();
        }

        private void GenerateAvailable() {
            var maxX = Files.Keys.Max(x => Math.Abs(x.x));
            var maxY = Files.Keys.Max(x => Math.Abs(x.y));
            var max = maxX > maxY ? maxX : maxY;
            for (var level = 0; level <= max; level++) {
                GenerateAvailableCircle(level);
            }
        }

        private void GenerateAvailableCircle(int level) {
            HashSet<(int x, int y)> items = new();
            //generate first row
            for (var i = -level; i <= level; i++) {
                items.Add((level, i));
                items.Add((-level, i));
                items.Add((i, -level));
                items.Add((i, level));
            }

            foreach (var item in items) {
                if (Files.ContainsKey(item)) {
                    Regions.Add(item);
                }
            }
        }

        private bool LoadRegion(int x, int y) {
            var position = (x, y);
            if (!Files.ContainsKey(position)) {
                return false;
            }

            const int headerSize = 8192;
            using var file = new BinaryReader(File.OpenRead(Files[position]));
            var data = new byte[file.BaseStream.Length - headerSize];
            var headerData = new byte[headerSize];
            file.Read(headerData, 0, headerSize);
            file.Read(data, 0, (int)file.BaseStream.Length - headerSize);
            RegionData.TryAdd(position, (HeaderReader.Parse(headerData), data));
            return true;
        }

        private bool LoadChunk(int x, int z) {
            (int x, int y) region = (x >> 5, z >> 5);
            if (!RegionData.ContainsKey(region)) {
                var regionResult = LoadRegion(region.x, region.y);
                if (!regionResult) {
                    return false;
                }
            }

            var chunkPosition = (x, z);
            var chunk = ReadChunk(chunkPosition, region);
            ChunkData.TryAdd(chunkPosition, chunk);
            return true;
        }

        public BlockType GetBlockAt(int x, int y, int z) {
            (int x, int y) chunk = ((int)Math.Floor(x / 16d), (int)Math.Floor(z / 16d));
            if (ChunkData.ContainsKey(chunk)) {
                return ChunkData[chunk].GetBlock(x, y, z);
            }

            var result = LoadChunk(chunk.x, chunk.y);
            if (!result) {
                return BlockType.NotGenerated;
            }

            return ChunkData[chunk].GetBlock(x, y, z);
        }

        public BlockType GetBlockAtTop(int x, int z) {
            var chunk = GetChunkAt(x, 0, z);
            var block = ((int)Math.Floor(x % 16d), (int)Math.Floor(z % 16d));
            if (!chunk.TopLayer.ContainsKey(block)) {
                return BlockType.NotGenerated;
            }

            return chunk.TopLayer[block].type;
        }


        public Chunk GetChunkAt(int x, int y, int z) {
            (int x, int y) chunk = ((int)Math.Floor(x / 16d), (int)Math.Floor(z / 16d));
            if (ChunkData.ContainsKey(chunk)) {
                return ChunkData[chunk];
            }

            var result = LoadChunk(chunk.x, chunk.y);
            if (!result)
                return null;
            else
                return ChunkData[chunk];
        }

        public Region ReadRegion(int xLocation, int yLocation) {
            var region = new Region(xOffset: xLocation, yOffset: yLocation);
            var startX = xLocation << 5;
            var startY = yLocation << 5;
            var endX = xLocation + 1 << 5;
            var endY = yLocation + 1 << 5;
            for (var x = startX; x < endX; x++) {
                for (var y = startY; y < endY; y++) {
                    region[Math.Abs(x) % 32, Math.Abs(y) % 32] = GetChunkAt(x * 16, 0, y * 16);
                }
            }

            region.Chunks = region.Chunks.OrderBy(x => x.XMin).ThenBy(x => x.ZMin).ToArray();
            return region;
        }

        private Chunk ReadChunk((int x, int y) position, (int x, int y) region) {
            var headerIndex = (position.x & 31) + (position.y & 31) * 32;
            var (headers, data) = RegionData[region];
            var offset = (int)headers[headerIndex].Offset;
            if (offset == 0) {
            }

            var count = 0;
            try {
                count = (data[offset] << 24) | data[offset + 1] << 16 | data[offset + 2] << 8 | data[offset + 3];
            } catch (Exception e) {
                Console.WriteLine(e);
                throw;
            }

            using var serializer = new NBTSerializer<RawRegion>(
                data.AsMemory(offset + 5, count - 1),
                (CompressionType)data[offset + 4]
            );
            var regionData = serializer.Serialize();
            List<Section> sections = new();
            foreach (var sectionData in regionData.Level.Sections) {
                if (sectionData.BlockStates == null) {
                    continue;
                }

                List<(BlockType, Dictionary<string, object>)> palette = sectionData.Palette
                    .Select(item => (EnumHelpers.BlockTypeFromName(item.Name), item.Properties)).ToList();

                sections.Add(new Section(sectionData.Y,
                    sectionData.BlockStates,
                    palette));
            }

            var chunksPositionInWorld = ((position.x * 16) + region.x * 32, (position.y * 16) + region.y * 32);
            return new Chunk(sections, chunksPositionInWorld);
        }
    }
}