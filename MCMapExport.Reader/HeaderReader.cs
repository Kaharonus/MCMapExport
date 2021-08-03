using System;
using System.Collections.Generic;
using System.Linq;
using MCMapExport.Reader.Models;

namespace MCMapExport.Reader {
    public static class HeaderReader {
        public static List<Header> Parse(byte[] data) {
            if (data.Length != 8192) {
                throw new ArgumentException("Data is not correct size");
            }

            List<Header> headers = new();
            for (var i = 0; i < 4096; i += 4) {
                var header = new Header {
                    SectorCount = data[i + 3],
                    //Data is read without the 2 block large offset - subtract 2 and blocks are 4096 bytes -> * 4096
                    Offset = (((uint) ((data[i] << 16) | data[i + 1] << 8 | data[i + 2])) - 2) * 4096,
                    //Not fastest, but readable
                    TimeStamp = (uint) ((data[i + 4096 + 0] << 24) |
                                        data[i + 4096 + 1] << 16 |
                                        data[i + 4096 + 2] << 8 |
                                        data[i + 4096 + 3] << 0)
                };
                headers.Add(header);
            }

            return headers;
        }
    }
}