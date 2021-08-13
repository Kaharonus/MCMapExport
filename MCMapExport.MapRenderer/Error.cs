using System;
using Avalonia.OpenGL;
using static Avalonia.OpenGL.GlConsts;

namespace MCMapExport.MapRenderer {

    public enum GlErrors {
        GL_INVALID_ENUM = 0x0500,
        GL_INVALID_VALUE = 0x0501,
        GL_INVALID_OPERATION = 0x0502,
        GL_STACK_OVERFLOW = 0x0503,
        GL_STACK_UNDERFLOW = 0x0504,
        GL_OUT_OF_MEMORY = 0x0505,
        GL_INVALID_FRAMEBUFFER_OPERATION = 0x0506,
        GL_CONTEXT_LOST = 0x0507,
        
    }
    
    public static class Error {
        public static bool CheckError(this GlInterface gl) {
            int err;
            var hasError = false;
            while ((err = gl.GetError()) != GL_NO_ERROR) {
                var enumValue = (GlErrors)err;
                Console.WriteLine($"OpenGL error detected: {enumValue} ({err})");
                hasError = true;
            }

            return hasError;

        }
    }
}