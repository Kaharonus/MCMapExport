namespace MCMapExport.Common {
    public static class Endianness {
        public static short SwapEndianness(this short x) {
            return (short) SwapEndianness((ushort) x);
        }

        public static int SwapEndianness(this int x) {
            return (int) SwapEndianness((uint) x);
        }

        public static long SwapEndianness(this long x) {
            return (long) SwapEndianness((uint) x);
        }

        public static ushort SwapEndianness(this ushort x) {
            return (ushort) ((ushort) ((x & 0xff) << 8) | ((x >> 8) & 0xff));
        }

        public static uint SwapEndianness(this uint x) {
            return ((x & 0x000000ff) << 24) +
                   ((x & 0x0000ff00) << 8) +
                   ((x & 0x00ff0000) >> 8) +
                   ((x & 0xff000000) >> 24);
        }

        public static ulong SwapEndianness(this ulong value) {
            var swapped =
                ((0x00000000000000FF) & (value >> 56)
                 | (0x000000000000FF00) & (value >> 40)
                 | (0x0000000000FF0000) & (value >> 24)
                 | (0x00000000FF000000) & (value >> 8)
                 | (0x000000FF00000000) & (value << 8)
                 | (0x0000FF0000000000) & (value << 24)
                 | (0x00FF000000000000) & (value << 40)
                 | (0xFF00000000000000) & (value << 56));
            return swapped;
        }
    }
}