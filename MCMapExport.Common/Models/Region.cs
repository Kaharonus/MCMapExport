using System.Collections.Generic;

namespace MCMapExport.Common.Models {
    public class Region {
        public Region() {
        }

        public Region(int xOffset, int yOffset) {
            XOffset = xOffset;
            YOffset = yOffset;
        }

        public Chunk this[int x, int y] {
            get => Chunks[32 * x + y];
            set => Chunks[32 * x + y] = value;
        }

        public int XOffset { get; set; }
        public int YOffset { get; set; }

        public Chunk[] Chunks { get; set; } = new Chunk[1024];
    }
}