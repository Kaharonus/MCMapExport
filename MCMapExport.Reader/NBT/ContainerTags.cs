using System;

namespace MCMapExport.Reader.NBT {
    public class ReferenceTypeTags {
        public class TagEnd : Tag<object> {
            public override TagType Type => TagType.TagEnd;


            public override object Payload {
                get => null;
                set { }
            }

            public override ITag this[object index] => throw new NotSupportedException();
        }
    }
}