using System;
using System.Collections.Generic;
using System.Linq;

namespace MCMapExport.Reader {
    public class Header {
        public uint TimeStamp { get; set; }
        public byte SectorCount { get; set; }
        public uint Offset { get; set; }

        public static IEnumerable<Header> Parse(byte[] data) {
            if (data.Length != 8192) {
                throw new ArgumentException("Data is not correct size");
            }
            List<Header> headers = new();
            for (var i = 0; i < 4096; i += 4) {
                var header = new Header {
                    SectorCount = data[i + 4],
                    Offset = (uint) ((data[i] << 16) | data[i + 1] << 8 | data[i + 2]),
                    TimeStamp = BitConverter.ToUInt32(data.AsSpan()[(i + 4096)..(i + 4096 + 4)])
                };
                headers.Add(header);
            }

            headers = headers.OrderBy(x => x.Offset).ToList();
            var test = headers.Where(x => x.SectorCount != 0);
            return headers;
        } 
    }
}