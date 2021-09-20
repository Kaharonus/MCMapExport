using System.Collections.Generic;

namespace MCMapExport.Reader.Models {
    public class Level {
        public int[] Biomes { get; set; }
        public RawSection[] Sections { get; set; }
    }
}