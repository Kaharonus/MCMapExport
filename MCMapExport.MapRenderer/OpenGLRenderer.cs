using System;
using System.IO;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using static Avalonia.OpenGL.GlConsts;

namespace MCMapExport.MapRenderer {
    public class OpenGLRenderer : OpenGlControlBase {

        public Camera Camera { get; private set; } = new();
        
        private string VertexShaderSource => ResourceLoader.GetShader("Shaders\\vertex.glsl");
        private string FragmentShaderSource => ResourceLoader.GetShader("Shaders\\fragment.glsl");

        private int _vertexShader;
        private int _fragmentShader;
        private int _shaderProgram;
        private int _vbo;
        private int _vao;
        private int _texture;
        private VAOInterface _glExt;
        
        
        private RgbaColor[] _textureData;
        private int _textureHeight;
        private int _textureWidth;
        private bool _textureUpdated = false;
        
        
        private float[] vertices = {
            0.5f, 0.5f, 0.0f,
            -0.5f, 0.5f, 0.0f,
            0.0f, -0.5f, 0.0f
        };

        
        public void SetTexture(RgbaColor[] data, int height, int width) {
            _textureData = data;
            _textureWidth = width;
            _textureHeight = height;
            _textureUpdated = true;
        }

        public void ScheduleRedraw() {
            Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
        }
        
        private unsafe void SendTextureToGPU(GlInterface gl) {
            fixed (void* ptr = _textureData) {
                gl.TexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, _textureWidth, _textureHeight, 0, GL_RGBA, GL_UNSIGNED_BYTE, new IntPtr(ptr));
            }
        }

        public void SetCamera(Camera cam) {
            Camera = cam;
        }
        
        
        protected override unsafe void OnOpenGlInit(GlInterface gl, int fb) {
            _glExt = new VAOInterface(gl);
            
            gl.CheckError();
            _vertexShader = gl.CreateShader(GL_VERTEX_SHADER);
            Console.WriteLine(gl.CompileShaderAndGetError(_vertexShader, VertexShaderSource));
            _fragmentShader = gl.CreateShader(GL_FRAGMENT_SHADER);
            Console.WriteLine(gl.CompileShaderAndGetError(_fragmentShader, FragmentShaderSource));
            
            _shaderProgram = gl.CreateProgram();
            gl.AttachShader(_shaderProgram, _vertexShader); 
            gl.AttachShader(_shaderProgram, _fragmentShader);
            Console.WriteLine(gl.LinkProgramAndGetError(_shaderProgram));

            gl.UseProgram(_shaderProgram);
            
            var bufferArr = new int[1];

            gl.GenBuffers(1, bufferArr);
            _vbo = bufferArr[0];
            gl.BindBuffer(GL_ARRAY_BUFFER, _vbo);
            fixed (void* verticesPtr = vertices) {
                gl.BufferData(GL_ARRAY_BUFFER, new IntPtr(vertices.Length * sizeof(float)), new IntPtr(verticesPtr), GL_STATIC_DRAW);
            }

            _vao = _glExt.GenVertexArray();
            _glExt.BindVertexArray(_vao);
            gl.VertexAttribPointer(0, 3, GL_FLOAT, 0, sizeof(float)*3, IntPtr.Zero);
            gl.EnableVertexAttribArray(0);

            gl.GenTextures(1, bufferArr);
            _texture = bufferArr[0];
            gl.BindTexture(GL_TEXTURE_2D, _texture);
            
            gl.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);	
            gl.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
            gl.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
            gl.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
            SendTextureToGPU(gl);

            gl.CheckError();
            
        }

        protected override unsafe void OnOpenGlRender(GlInterface gl, int fb) {
                gl.ClearColor(0, 0, 0, 0);
                gl.Clear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
                gl.Enable(GL_DEPTH_TEST);
                gl.Viewport(0, 0, (int)Bounds.Width, (int)Bounds.Height);
            
            
                var camX = gl.GetUniformLocationString(_shaderProgram, "camX");
                var camY = gl.GetUniformLocationString(_shaderProgram, "camY");
                var camZoom = gl.GetUniformLocationString(_shaderProgram, "camZoom");
                var aspectRatio = gl.GetUniformLocationString(_shaderProgram, "aspectRatio");
            
                gl.UseProgram(_shaderProgram);
                _glExt.BindVertexArray(_vao);
                gl.BindTexture(GL_TEXTURE_2D, _texture);
                if (_textureUpdated) {
                    SendTextureToGPU(gl);
                    _textureUpdated = false;
                }

                gl.Uniform1f(camX, Camera.X);
                gl.Uniform1f(camY, Camera.Y);
                gl.Uniform1f(camZoom, Camera.Zoom);
                gl.Uniform1f(aspectRatio, (float)(Bounds.Width / Bounds.Height));


                gl.DrawArrays(GL_TRIANGLES, 0, new IntPtr(3));
                gl.CheckError();
                ScheduleRedraw();
                //GenerateData();
        }
        
        
     
        

    }
}