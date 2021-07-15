using System;
using System.IO;
using System.Linq;

namespace MCMapExport.Reader {
    public class WorldReader {

        private readonly string _directory;

        public static WorldReader? Open(string path, out string error) {
            error = "";
            if (!Directory.Exists(path)) {
                error = $"Directory {path} does not exist";
                return null;
            }

            var dirs = Directory.GetDirectories(path);
            var subDirs = new []{"DIM-1", "DIM1", "region"};
            if (!dirs.Select(Path.GetFileName).ContainsAll(subDirs)) {
                error = $"Directory {path} does not contain all required subdirectories";
                return null;
            }
            var files = Directory.GetFiles(path);
            if (!files.Select(Path.GetFileName).Contains("level.dat")) {
                error = $"File {path}/file.dat not found";
                return null;
            }
            return new WorldReader(path);
        }
        
        
        private WorldReader(string directory) {
            _directory = directory;
        }
        
        public void Read() {
            var files = Directory.GetFiles(_directory + "/region/").OrderBy(x=>x);
            foreach (var filePath in files) {
                using var file = new BinaryReader(File.OpenRead(filePath));
                byte[] header = new byte[8192];
                file.Read(header, 0, 8192);
                var headers = Header.Parse(header);
              
            }
            
          
        }
    }
}