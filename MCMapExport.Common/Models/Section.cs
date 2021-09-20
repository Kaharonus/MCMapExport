using System;
using System.Collections.Generic;
using System.Linq;
using MCMapExport.Common.Enums;

namespace MCMapExport.Common.Models {
    public class Section {
        public byte[] BlockStates { get; }
        public List<(BlockType type, Dictionary<string, object> properties)> Palette { get; }

        public int Index { get; }

        private readonly int _bits;
        private readonly int _blocksPerWord;
        private readonly int _mask;

        public Section(int index, byte[] blockStates, IEnumerable<(BlockType, Dictionary<string, object>)> palette) {
            Palette = palette.ToList();
            BlockStates = blockStates;
            _bits = 4;
            while ((1u << _bits) < Palette.Count) {
                _bits++;
            }
            _blocksPerWord =  64 / _bits;
            _mask = (1 << _bits) - 1;

            Index = index;
        }



        public BlockType GetTypeAt(int x, int y, int z) {
            var index = (y << 8) + (z << 4) + x;
            return GetTypeAt(index);
        }
        
        private static int MangleByteIndex(int index) {
            return (index & ~7) + 7 - (index & 7);
        }

        public BlockType GetTypeAt(int index) {
            
            //Shamelessly stolen/ported from https://github.com/NeoRaider/MinedMap and their C++ implementation.
            //I would probably never figure this shit out without it.
            var bitIndex = 64 * (index / _blocksPerWord) + _bits * (index % _blocksPerWord);
            var pos = bitIndex >> 3;
            var shift = bitIndex & 7;
            int typeByte = BlockStates[MangleByteIndex(pos)];

            if (shift + _bits > 8) {
                typeByte |= BlockStates[MangleByteIndex(pos + 1)] << 8;
            }

            if (shift + _bits > 16) {
                typeByte |= BlockStates[MangleByteIndex(pos + 2)] << 16;
            }

            return Palette.ElementAt((typeByte >> shift) & _mask).type;
        }
    }
}