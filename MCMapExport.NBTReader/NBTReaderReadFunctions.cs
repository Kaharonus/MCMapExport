using System;
using System.Diagnostics;

namespace MCMapExport.NBT {
    public partial class NBTReader {
        private unsafe float ReadFloat() {
            var value = ReadInt();
            return *(float*)&value;
        }

        private unsafe double ReadDouble() {
            var value = ReadLong();
            return *(double*)&value;
        }
        
        private int ReadInt() {
            var b1 = ReadByte();
            var b2 = ReadByte();
            var b3 = ReadByte();
            var b4 = ReadByte();
            if (BitConverter.IsLittleEndian) {
                return  (b1 << 24) + (b2 << 16) + (b3 << 8) + b4;
            }
            return (b4 << 24) + (b3 << 16) + (b2 << 8) + b1;

        }

        private short ReadShort() {
            var b1 = ReadByte();
            var b2 = ReadByte();
            if (BitConverter.IsLittleEndian) {
                return (short)((b1 << 8) + b2);
            }
            return (short)((b2 << 8) + b1);
        }

        private long ReadLong() {
            var b1 = ReadByte();
            var b2 = ReadByte();
            var b3 = ReadByte();
            var b4 = ReadByte();
            var b5 = ReadByte();
            var b6 = ReadByte();
            var b7 = ReadByte();
            var b8 = ReadByte();
            if (BitConverter.IsLittleEndian) {
                return ((long)((b1 << 24) + (b2 << 16) + (b3 << 8) + b4) << 32) + 
                       ((b5 << 24) + (b6 << 16) + (b7 << 8) + b8);
            }
            return ((long)((b8 << 24) + (b7 << 16) + (b6 << 8) + b5) << 32) + 
                   ((b4 << 24) + (b3 << 16) + (b2 << 8) + b1);
        }
        
        private byte ReadByte() {
            return (byte)_data.ReadByte();
        }
    }
}