using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MCMapExport.Common.Models;
using MCMapExport.Reader.Models;
using MCMapExport.Reader.NBT;

namespace MCMapExport.Reader {
    public class ChunkReader {

        public static Chunk ReadChunk(RawChunk raw) {
            using var stream = new MemoryStream(raw.DecompressedData);
            //This might fail for many reasons.
            var baseTag = (TagCompound)NBTReader.GetTag(stream);
            var sections = (TagList)baseTag["Level"]["Sections"];
            foreach (var compound in sections.ItemsAs<TagCompound>()) {
                if (!compound.Contains("BlockStates")) {
                    continue;
                }

                var states = compound["BlockStates"] as TagLongArray;
                
            }

            return default;
        }
        
    }
}