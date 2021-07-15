using System.Collections.Generic;

namespace MCMapExport.Common.Models {
    public class Region {
        public int Top { get; init; }
        public int Left { get; init; }
        public int Right { get; init; }
        public int Bottom { get; init; }
        
        public IEnumerable<Chunk> Chunks { get; init; } = new List<Chunk>();

    }
}