namespace MCMapExport.Reader.Models {
    public class RawSection {
        public sbyte Y { get; set; }
        public byte[] BlockStates { get; set; }
        public Palette[] Palette { get; set; }
    }
}