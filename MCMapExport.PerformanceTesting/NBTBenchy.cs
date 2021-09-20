using System.IO;
using System.Net;
using BenchmarkDotNet.Attributes;
using MCMapExport.NBT;
using MCMapExport.NBT.Tags;
using MCMapExport.Reader.Models;

namespace MCMapExport.PerformanceTesting {
    [MemoryDiagnoser]
    public class NBTBenchy {
        private NBTSerializer<RawRegion> _serializer;
        private NBTReader _reader;

        [GlobalSetup]
        public void Setup() {
            const string file = @"../../../../test.nbt";
            var bytes = File.ReadAllBytes(file);
            _serializer = new NBTSerializer<RawRegion>(bytes, CompressionType.Uncompressed);
            _reader = new NBTReader(bytes, CompressionType.Uncompressed);
        }
        [Benchmark]
        public RawRegion Serializer() {
            return _serializer.Serialize();
        }

        [Benchmark]
        public CompoundTag Reader() {
            return (CompoundTag)_reader.GetTag();
        }
        
       
        
    }
}