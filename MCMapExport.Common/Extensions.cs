using System.Collections.Generic;
using System.Linq;

namespace MCMapExport {
    public static class Extensions {
        
        public static bool ContainsAll<T>(this IEnumerable<T> source, IEnumerable<T> values) {
            return values.All(source.Contains);
        }

        public static byte GetNibble(this byte b, bool fromTop = false) {
            return (byte)(fromTop ? (b >> 4) & 0x0F : b & 0x0F);
        }
        
    }
}