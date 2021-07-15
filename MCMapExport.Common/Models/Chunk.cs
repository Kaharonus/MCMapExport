using System.Collections.Generic;
using System.Numerics;

namespace MCMapExport.Common.Models {
    public class Chunk {
        public int Top { get; init; }
        public int Left { get; init; }
        public int Right { get; init; }
        public int Bottom { get; init; }
        private IDictionary<Vector2, Block[]> _blocks = new Dictionary<Vector2, Block[]>();

        public Block GetBlock(Vector3 position) {


            return default;
        }

        public void AddBlock(Block block) {
            
        }
        
        //public IEnumerable<Block> Blocks { get; init; } = new List<Block>();
    }
}