using System;

namespace MCMapExport.Reader.NBT {
    public abstract class ValueTag<T> : ITag {
        public ITag this[object index] => throw new NotSupportedException();
        
        
        
        public string Name { get; set; }

        public abstract T Payload { get; init; }
        public abstract TagType Type { get; }
        public long Depth { get; set; }
        public object PayloadGeneric => Payload;
        public bool IsValue => true;
    }


    public class TagEnd : ValueTag<object> {
        
        public override TagType Type => TagType.TagEnd;

        public override object Payload {
            get => null;
            init { }
        }
    }


    public class TagString : ValueTag<string> {
        public override TagType Type => TagType.TagString;

        public override string Payload { get; init; }
    }

    public class TagByte : ValueTag<sbyte> {
        public override TagType Type => TagType.TagByte;

        public override sbyte Payload { get; init; }
    }

    public class TagShort : ValueTag<short> {
        public override TagType Type => TagType.TagShort;

        public override short Payload { get; init; }
        
    }

    public class TagInt : ValueTag<int> {
        public override TagType Type => TagType.TagInt;

        public override int Payload { get; init; }
    }

    public class TagLong : ValueTag<long> {
        public override TagType Type => TagType.TagLong;

        public override long Payload { get; init; }
    }

    public class TagFloat : ValueTag<float> {
        public override TagType Type => TagType.TagFloat;

        public override float Payload { get; init; }
    }

    public class TagDouble : ValueTag<double> {
        public override TagType Type => TagType.TagDouble;

        public override double Payload { get; init; }
    }
}