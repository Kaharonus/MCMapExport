using System;
using System.IO;
using System.Reflection;

namespace MCMapExport.MapRenderer {
    public class ResourceLoader {

        private static readonly Assembly Assembly = Assembly.GetAssembly(typeof(ResourceLoader));
        private static readonly string AssemblyName = Assembly.GetName().Name;
        
        public static string ReadFile(string name) {
            name = $"{AssemblyName}.{name}";
            using var stream = Assembly.GetManifestResourceStream(name);
            if (stream == null) {
                return null;
            }

            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        
        public static string GetShader(string shaderPath) {
            shaderPath = shaderPath.Replace("\\", ".");
            var shader = ResourceLoader.ReadFile(shaderPath);
            if (string.IsNullOrEmpty(shader)) {
                Console.WriteLine($"File {shaderPath} was empty or doesnt exist");
                throw new ArgumentException("File was empty or doesnt exist");
            }
            return shader;
        }
    }
}