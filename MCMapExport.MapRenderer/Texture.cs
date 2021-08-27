using System;
using Avalonia.OpenGL;
using MCMapExport.MapRenderer.Utilities;
using static Avalonia.OpenGL.GlConsts;


namespace MCMapExport.MapRenderer {
    public class Texture {
        public int Height { get; private set; }
        public int Width { get; private set; }

        public int XOffset { get; set; }
        public int YOffset { get; set; }
        
        private RgbaColor[] Data { get; set; }

        public bool IsChanged = false;
        public bool IsCreated = false;
        public int TextureName;

        public RgbaColor this[int row, int col] {
            get => Data[Width * row + col];
            set {
                Data[Width * row + col] = value;
                IsChanged = true;
            }
        }

        public Texture(int dimensions, int xOffset, int yOffset) {
            Height = dimensions;
            XOffset = xOffset;
            YOffset = yOffset;
            Width = dimensions;
            Data = new RgbaColor[Width*Height];
        }

        public void SetDimensions(int dimensions) {
            Height = Width = dimensions;
            Data = new RgbaColor[Height * Width];
        }

        public void Create(GlInterface gl) {
            var bufferArr = new int[1];
            gl.GenTextures(1, bufferArr);
            TextureName = bufferArr[0];
            IsCreated = true;
        }

        public unsafe void UploadData(GlInterface gl) {
            
            gl.BindTexture(GL_TEXTURE_2D, TextureName);  
            fixed (void* ptr = Data) {
                gl.TexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, Width, Height, 0, GL_RGBA, GL_UNSIGNED_BYTE,
                    new IntPtr(ptr));
            }
            gl.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
            gl.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
            gl.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
            gl.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
            IsChanged = false;
            
        }

        public void Activate(GlInterface gl) {
            if (!IsCreated) {
                return;
            }
            gl.BindTexture(GL_TEXTURE_2D, TextureName);
        }
        
    }
}