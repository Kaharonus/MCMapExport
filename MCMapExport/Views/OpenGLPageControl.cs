using System;
using System.Diagnostics;
using System.IO;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using MCMapExport.OpenGL;
using static Avalonia.OpenGL.GlConsts;

namespace MCMapExport.Views {
    public class OpenGLPageControl : OpenGlControlBase {

        
        private string VertexShaderSource => GetShader(false, "OpenGL/vertex.glsl");
        
        private string FragmentShaderSource => GetShader(true, "OpenGL/fragment.glsl");

        private int _vertexShader;
        private int _fragmentShader;
        private int _shaderProgram;
        private int _vbo;
        private int _vao;
        private VAOInterface _glExt;
        

        private float[] vertices = {
            -0.5f, -0.5f, 0.0f,
            0.5f, -0.5f, 0.0f,
            0.0f, 0.5f, 0.0f
        };
        
        protected override unsafe void OnOpenGlInit(GlInterface gl, int fb) {
            _glExt = new VAOInterface(gl);
            
            CheckError(gl);
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
            CheckError(gl);        }


        protected override unsafe void OnOpenGlRender(GlInterface gl, int fb) {
            gl.ClearColor(0, 0, 0, 0);
            gl.Clear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
            gl.Enable(GL_DEPTH_TEST);
            gl.Viewport(0, 0, (int)Bounds.Width, (int)Bounds.Height);
            
            gl.UseProgram(_shaderProgram);
            _glExt.BindVertexArray(_vao);
            
            gl.DrawArrays(GL_TRIANGLES, 0, new IntPtr(3));
            CheckError(gl);
        }
        
        
        private void CheckError(GlInterface gl) {
            int err;
            while ((err = gl.GetError()) != GL_NO_ERROR)
                Console.WriteLine(err);
        }
        
        private string GetShader(bool fragment, string shaderPath) {
            if (!File.Exists(shaderPath)) {
                Console.WriteLine($"File {shaderPath} does not exist");
                throw new ArgumentException("File does not exist");
            }
            var shader = File.ReadAllText(shaderPath);
            if (string.IsNullOrEmpty(shader)) {
                Console.WriteLine($"File {shaderPath} was empty");
                throw new ArgumentException("File was empty");
            }

            return shader;
        }
        

    }
}