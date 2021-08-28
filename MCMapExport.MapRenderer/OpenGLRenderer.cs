using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using MCMapExport.MapRenderer.Utilities;
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
        private int _ebo;
        private float _aspectRatio;
        private VAOInterface _glExt;
        private Dictionary<(int x, int y), Texture> _textures = new();

        public void AddTexture(int x, int y, Texture tex) {
            lock (_textures) {
                _textures.Add((x, y), tex);
            }
        }


        public void ScheduleRedraw() {
            Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
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
            fixed (void* verticesPtr = VertexData.Vertices) {
                gl.BufferData(GL_ARRAY_BUFFER, new IntPtr(VertexData.Vertices.Length * sizeof(float)),
                    new IntPtr(verticesPtr), GL_STATIC_DRAW);
            }


            gl.GenBuffers(1, bufferArr);
            _ebo = bufferArr[0];
            gl.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, _ebo);
            fixed (void* indicesPtr = VertexData.Indices) {
                gl.BufferData(GL_ELEMENT_ARRAY_BUFFER, new IntPtr(VertexData.Indices.Length * sizeof(int)),
                    new IntPtr(indicesPtr), GL_STATIC_DRAW);
            }

            _vao = _glExt.GenVertexArray();
            _glExt.BindVertexArray(_vao);
            gl.VertexAttribPointer(0, 3, GL_FLOAT, 0, sizeof(float) * 5, IntPtr.Zero);
            gl.VertexAttribPointer(1, 2, GL_FLOAT, 0, sizeof(float) * 5, new IntPtr(3 * sizeof(float)));

            gl.EnableVertexAttribArray(0);
            gl.EnableVertexAttribArray(1);

            gl.CheckError();
            StartRender();
        }

        private void StartRender() {
            var t = new Timer(16);
            t.Elapsed += (s, ev) => ScheduleRedraw();
            t.Start();
        }

        private void DrawRectangle(int xOffset, int yOffset, GlInterface gl, int fb) {
            var camX = gl.GetUniformLocationString(_shaderProgram, "camX");
            var camY = gl.GetUniformLocationString(_shaderProgram, "camY");
            var camZoom = gl.GetUniformLocationString(_shaderProgram, "camZoom");
            var aspectRatio = gl.GetUniformLocationString(_shaderProgram, "aspectRatio");
            var xOffsetLocation = gl.GetUniformLocationString(_shaderProgram, "xOffset");
            var yOffsetLocation = gl.GetUniformLocationString(_shaderProgram, "yOffset");


            gl.UseProgram(_shaderProgram);
            _glExt.BindVertexArray(_vao);

            gl.Uniform1f(camX, Camera.X);
            gl.Uniform1f(camY, Camera.Y);
            gl.Uniform1f(camZoom, Camera.Zoom);
            gl.Uniform1f(aspectRatio, _aspectRatio);
            gl.Uniform1f(xOffsetLocation, xOffset * 2f);
            gl.Uniform1f(yOffsetLocation, yOffset * 2f);

            gl.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, _ebo);
            gl.DrawElements(GL_TRIANGLES, VertexData.Indices.Length, GL_UNSIGNED_INT, new IntPtr(0));
        }

        protected override unsafe void OnOpenGlRender(GlInterface gl, int fb) {
            gl.CheckError();

            gl.ClearColor(0, 0, 0, 0);
            gl.Clear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
            gl.Enable(GL_DEPTH_TEST);
            gl.Viewport(0, 0, (int)Bounds.Width, (int)Bounds.Height);
            _aspectRatio = (float)(Bounds.Width / Bounds.Height);


            foreach (var ((x, y), texture) in _textures) {
                if (!texture.IsCreated) {
                    texture.Create(gl);
                    texture.UploadData(gl);
                } else if (texture.IsChanged) {
                    texture.UploadData(gl);
                }

                texture.Activate(gl);
                DrawRectangle(x, y, gl, fb);
            }

            gl.CheckError();
        }
    }
}