using System;

namespace MCMapExport.NBT.Tags {
    public abstract class ValueTag<T> : ITag {
        public static implicit operator T(ValueTag<T> t) {
            return t.Payload;
        }

        public T Payload { get; set; }

        public string Name => string.Empty;

        public ITag this[object index] => throw new NotSupportedException();
        
        public object PayloadGeneric => Payload;
        public bool IsValue => true;
    }


    public class EndTag : ValueTag<object> {
        public new object Payload {
            get => null;
            init { }
        }
    }


    public class StringTag : ValueTag<string> {
    }

    public class ByteTag : ValueTag<byte> {
    }

    public class ShortTag : ValueTag<short> {
    }

    public class IntTag : ValueTag<int> {
    }

    public class LongTag : ValueTag<long> {
    }

    public class FloatTag : ValueTag<float> {
    }

    public class DoubleTag : ValueTag<double> {
    }
}