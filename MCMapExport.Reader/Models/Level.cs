using System.Collections.Generic;

namespace MCMapExport.Reader.Models {
    public class Level {
        public List<int> Biomes { get; set; }
        public List<RawSection> Sections { get; set; }
    }
}