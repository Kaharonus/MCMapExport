using System.Collections.Generic;
using System.Linq;

namespace MCMapExport {
    public static class Extensions {
        
        public static bool ContainsAll<T>(this IEnumerable<T> source, IEnumerable<T> values) {
            return values.All(source.Contains);
        }
    }
}