using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using MCMapExport.Common;
using MCMapExport.Common.Enums;
using MCMapExport.Common.Models;
using MCMapExport.NBT;
using MCMapExport.NBT.Tags;
using MCMapExport.Reader.Models;

namespace MCMapExport.Reader {
    public class WorldReader {
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

        public string SavePath { get; }
        private Dictionary<Vector2, (List<Header> headers, byte[] data)> RegionData { get; set; } = new();
        private Dictionary<Vector2, Chunk> ChunkData { get; set; } = new();
        private Dictionary<Vector2, string> Files { get; } = new();

        private readonly NBTReaderConfiguration _readerConfiguration = new() {
            UseIntArrays = false,
            UseLongArrays = false
        };
        
        public WorldReader(string path) {
            SavePath = path;
            var files = Directory.GetFiles(SavePath + "/region/").OrderBy(x => x);
            var names = files.Select(
                x => (x, Regex.Match(x, @"-?[0-9]+\.-?[0-9]+\.mca$").Value.Split("."))
            );
            foreach (var (s, parts) in names) {
                var x = int.Parse(parts[0]);
                var z = int.Parse(parts[1]);
                Files.Add(new Vector2(x, z), s);
            }
        }

        public bool LoadRegion(Vector2 region) {
            if (!Files.ContainsKey(region)) {
                return false;
            }

            const int headerSize = 8192;
            using var file = new BinaryReader(File.OpenRead(Files[region]));
            var data = new byte[file.BaseStream.Length - headerSize];
            var headerData = new byte[headerSize];
            file.Read(headerData, 0, headerSize);
            file.Read(data, 0, (int) file.BaseStream.Length - headerSize);
            RegionData.Add(region, (HeaderReader.Parse(headerData), data));
            return true;
        }
        
        public bool LoadChunk(Vector2 chunkPosition) {
            var region = new Vector2((int) chunkPosition.X >> 5, (int) chunkPosition.Y >> 5);
            if (!RegionData.ContainsKey(region)) {
                var regionResult = LoadRegion(region);
                if (!regionResult) {
                    return false;
                }
            }
            var chunk = ReadChunk(chunkPosition, region);
            ChunkData.Add(chunkPosition, chunk);
            return true;
        }

        public BlockType GetBlockAt(Vector3 position) {
            var chunk = new Vector2((int) Math.Floor(position.X / 16d), (int) Math.Floor(position.Z / 16d));
            if (ChunkData.ContainsKey(chunk)) {
                return ChunkData[chunk].GetBlock(position);
            }

            var result = LoadChunk(chunk);
            if (!result) {
                return BlockType.NotGenerated;
            }

            return ChunkData[chunk].GetBlock(position);
        }

        public BlockType GetBlockAt(int x, int y, int z) {
            return GetBlockAt(new Vector3(x, y, z));
        }

        public Chunk GetChunkAt(int x, int y, int z) {
            return GetChunkAt(new Vector3(x, y, z));
        }
        
        public Chunk GetChunkAt(Vector3 block) {
            var chunk = new Vector2((int) Math.Floor(block.X / 16d), (int) Math.Floor(block.Z / 16d));
            if (ChunkData.ContainsKey(chunk)) {
                return ChunkData[chunk];
            }

            var result = LoadChunk(chunk);
            return !result ? null : ChunkData[chunk];
        }
        
        private Chunk ReadChunk(Vector2 position, Vector2 region) {
            var headerIndex = ((int)position.X & 31) + ((int)position.Y & 31) * 32;
            var (headers, data) = RegionData[region];
            var offset = (int)headers[headerIndex].Offset;
            var count = (data[offset] << 24) | data[offset + 1] << 16 | data[offset + 2] << 8 | data[offset + 3];
            using var reader = new NBTReader(
                data.AsMemory(offset + 5, count - 1),
                (CompressionType) data[offset + 4],
                _readerConfiguration
            );
            var baseTag = (CompoundTag) reader.GetTag();
            var sectionTag = (ListTag) baseTag["Level"]["Sections"];
            List<Section> sections = new();
            foreach (var sectionData in sectionTag.ItemsAs<CompoundTag>()) {
                if (!sectionData.Contains("BlockStates")) {
                    continue;
                }
                var states = sectionData.Get<ByteArrayTag>("BlockStates");
                var paletteItems = sectionData.Get<ListTag>("Palette").ItemsAs<CompoundTag>();
                List<(BlockType, Dictionary<string, object>)> palette = new();
                foreach (var item in paletteItems) {
                    Dictionary<string, object> properties = null;
                    if (item.Contains("Properties")) {
                        properties = item.Get<CompoundTag>("Properties")
                            .ToDictionary(key => key.Key, value => value.Value.PayloadGeneric);
                    }

                    palette.Add((EnumHelpers.BlockTypeFromName(item.Get<StringTag>("Name")), properties));
                }

                sections.Add(new Section(sectionData.Get<ByteTag>("Y"),
                    states.Payload.Select(x => (byte) x.Payload).ToList(), palette));
            }

            var chunksPositionInWorld = (position * 16) + region * 32;
            return new Chunk(sections,chunksPositionInWorld);
        }
    }
}