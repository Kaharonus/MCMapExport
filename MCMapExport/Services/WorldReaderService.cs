using System;
using System.Collections.Generic;
using System.IO;
using MCMapExport.MapRenderer;
using MCMapExport.NBT;
using MCMapExport.Reader;

namespace MCMapExport.Services {
    public class WorldReaderService {
        public WorldReader? Reader { get; set; }

        
        public bool IsInitialized => Reader is not null;

        public bool SetLocation(string location, out string error) {
            var reader = WorldReader.Create(location, out error);
            if (!string.IsNullOrEmpty(error)) {
                return false;
            }

            Reader = reader;
            return true;
        }
        
    }
}