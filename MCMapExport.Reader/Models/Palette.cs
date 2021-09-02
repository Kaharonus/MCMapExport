using System.Collections.Generic;

namespace MCMapExport.Reader.Models {
    public class Palette {
        public string Name { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }
}