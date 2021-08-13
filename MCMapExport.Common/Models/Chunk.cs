using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MCMapExport.Common.Enums;

namespace MCMapExport.Common.Models {
    public class Chunk {
        public int XMin { get; init; }
        public int XMax { get; init; }
        public int ZMin { get; init; }
        public int ZMax { get; init; }

        private readonly List<Section> _sections;

        public bool IsEmpty { get; private set; }
        

        public static Chunk Empty(Vector2 position) => new Chunk(null, position , true);

        public Dictionary<Vector2, (int y, BlockType type)> TopLayer { get; set; } = new();

        public Chunk(IEnumerable<Section> sections, Vector2 position, bool cacheTopmostLayer = true) {
            XMin = (int) position.X;
            XMax = (int) position.X + 15;
            ZMin = (int) position.Y;
            ZMax = (int) position.Y + 15;

            if (sections == null || !sections.Any()) {
                IsEmpty = true;
                return;
            }
            
            _sections = sections.OrderBy(x => x.Index).ToList();
            
            if (cacheTopmostLayer) {
                BuildCache();
            }
        }


        public BlockType GetBlock(Vector3 location) {
            if (IsEmpty) {
                return BlockType.NotGenerated;
            }
            var vec2 = new Vector2(location.X, location.Z);
            if (TopLayer.ContainsKey(vec2) && Math.Abs(TopLayer[vec2].y - location.Y) < 0.1) {
                return TopLayer[vec2].type;
            }

            var sectionIndex = ((int) location.Y / 16);
            var section = _sections[sectionIndex];
            return section.GetTypeAt((int) location.X, (int) location.Y % 16, (int) location.Z);
        }


        private void CacheSection(Section section) {
            const int size = 16;
            for (var x = 0; x < size; x++) {
                for (var z = 0; z < size; z++) {
                    for (var y = size - 1; y >= 0; y--) {
                        var type = section.GetTypeAt(x, y, z);
                        if (type is BlockType.Air or BlockType.CaveAir) {
                            continue;
                        }

                        var location = new Vector2(x, z);
                        if (TopLayer.ContainsKey(location)) {
                            continue;
                        }

                        var chunkY = size * section.Index + y;
                        TopLayer.Add(new Vector2(x, z), (chunkY, type));
                        break;
                    }
                }
            }
        }

        private void BuildCache() {
            //The great pyramid of Chunk.cs
            var sections = _sections.Where(x => x.Palette.Count > 1);

            foreach (var section in sections.Reverse()) {
                CacheSection(section);
                if (TopLayer.Count == 256) {
                    break;
                }
            }
        }
    }
}