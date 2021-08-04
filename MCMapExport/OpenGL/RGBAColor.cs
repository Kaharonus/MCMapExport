using System;
using System.Drawing;

namespace MCMapExport.OpenGL {
    public readonly struct RgbaColor {
        public readonly byte R;
        public readonly byte G;
        public readonly byte B;
        public readonly byte A;
        
        private RgbaColor(byte r, byte g, byte b, byte a) {
            R = r;
            G = g;
            B = b;
            A = a;
        }
        
        public static RgbaColor FromColor(Color c) {
            return new(c.R, c.G, c.B, c.A);
        }

        public static RgbaColor FromRgb(byte r, byte g, byte b) {
            return new(r, g, b, 255);
        }

        public static RgbaColor FromRgba(byte r, byte g, byte b, byte a) {
            return new(r, g, b, a);
        }

    }
}