using System;

namespace MCMapExport.Reader.NBT {
    public class ValueTypeTags {
        public abstract class ValueTag<T> : ITag {
            public ITag this[object index] => throw new NotSupportedException();

            public string Name { get; set; }
            public TagType Type { get; }
            public long Depth { get; set; }
            public object PayloadGeneric { get; }
        }
        
        
        public class TagString : Tag<string> {
            public override TagType Type => TagType.TagString;

            public override string Payload { get; set; }

            public override ITag this[object index] => throw new NotSupportedException();
        }

        public class TagByte : Tag<sbyte> {
                public override TagType Type => TagType.TagByte;
        
                public override sbyte Payload { get; set; }
        
                public override ITag this[object index] => throw new NotSupportedException();
            }
        
            public class TagShort : Tag<short> {
                public override TagType Type => TagType.TagShort;
        
                public override short Payload { get; set; }
        
                public override ITag this[object index] => throw new NotSupportedException();
            }
        
            public class TagInt : Tag<int> {
                public override TagType Type => TagType.TagInt;
        
                public override int Payload { get; set; }
        
                public override ITag this[object index] => throw new NotSupportedException();
            }
        
            public class TagLong : Tag<long> {
                public override TagType Type => TagType.TagLong;
        
                public override long Payload { get; set; }
        
                public override ITag this[object index] => throw new NotSupportedException();
            }
        
            public class TagFloat : Tag<float> {
                public override TagType Type => TagType.TagFloat;
        
                public override float Payload { get; set; }
        
                public override ITag this[object index] => throw new NotSupportedException();
            }
        
            public class TagDouble : Tag<double> {
                public override TagType Type => TagType.TagDouble;
        
                public override double Payload { get; set; }
        
                public override ITag this[object index] => throw new NotSupportedException();
            }
    }
}