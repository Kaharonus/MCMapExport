using System;

namespace MCMapExport.Reader.Models {
    public class Header {
        public uint TimeStamp { get; init; }
        public byte SectorCount { get; init; }
        public uint Offset { get; init; }
        
    }
}