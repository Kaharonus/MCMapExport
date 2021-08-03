using System;

namespace MCMapExport.NBT.Tags {
    public abstract class ValueTag<T> : ITag {
        public static implicit operator T(ValueTag<T> t) {
            return t.Payload;
        }

        public T Payload { get; init; }

        public ITag this[object index] => throw new NotSupportedException();

        public string Name { get; set; }

        public abstract TagType Type { get; }
        public long Depth { get; set; }
        public object PayloadGeneric => Payload;
        public bool IsValue => true;
    }


    public class EndTag : ValueTag<object> {
        public override TagType Type => TagType.TagEnd;

        public new object Payload {
            get => null;
            init { }
        }
    }


    public class StringTag : ValueTag<string> {
        public override TagType Type => TagType.TagString;
    }

    public class ByteTag : ValueTag<sbyte> {
        public override TagType Type => TagType.TagByte;
    }

    public class ShortTag : ValueTag<short> {
        public override TagType Type => TagType.TagShort;
    }

    public class IntTag : ValueTag<int> {
        public override TagType Type => TagType.TagInt;
    }

    public class LongTag : ValueTag<long> {
        public override TagType Type => TagType.TagLong;
    }

    public class FloatTag : ValueTag<float> {
        public override TagType Type => TagType.TagFloat;
    }

    public class DoubleTag : ValueTag<double> {
        public override TagType Type => TagType.TagDouble;
    }
}