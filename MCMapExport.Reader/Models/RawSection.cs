namespace MCMapExport.Reader.Models {
    public class RawSection {
        public byte Y { get; set; }
        public byte[] BlockStates;
        public Palette[] Palette { get; set; }
    }
}