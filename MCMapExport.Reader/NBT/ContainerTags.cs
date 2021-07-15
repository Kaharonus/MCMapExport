using System;
using System.Collections.Generic;
using System.Linq;

namespace MCMapExport.Reader.NBT {
    public abstract class ContainerTag<T> : ITag {
        public string Name { get; set; }
        public abstract TagType Type { get; }

        public abstract T Payload { get; set; }

        public abstract ITag this[object index] { get; }

        public object PayloadGeneric => Payload;

        public long Depth { get; set; }
    }

    
    public class TagCompound : ContainerTag<IDictionary<string, ITag>> {
        public override TagType Type => TagType.TagCompound;

        public override IDictionary<string, ITag> Payload { get; set; }

        public bool Contains(string key) {
            return Payload.ContainsKey(key);
        }

        public override ITag this[object index] => index is string i
            ? Payload[i]
            : throw new ArgumentException("String must be used when accessing a dictionary");
    }

    public class TagByteArray : ContainerTag<IEnumerable<TagByte>> {
        public override TagType Type => TagType.TagByteArray;

        public override IEnumerable<TagByte> Payload { get; set; }

        public override ITag this[object index] =>
            index is int i ? Payload.ElementAt(i) : throw new ArgumentException("Int must be used as an index");
    }


    public class TagList : ContainerTag<IEnumerable<ITag>> {
        public override TagType Type => TagType.TagList;
        public override IEnumerable<ITag> Payload { get; set; }

        public IEnumerable<T> ItemsAs<T>() {
            return Payload.Select(x => (T) x);
        }

        public override ITag this[object index] =>
            index is int i ? Payload.ElementAt(i) : throw new ArgumentException("Int must be used as an index");

        public TagType ListType { get; set; }
    }

    public class TagIntArray : ContainerTag<IEnumerable<TagInt>> {
        public override TagType Type => TagType.TagIntArray;

        public override IEnumerable<TagInt> Payload { get; set; }

        public override ITag this[object index] =>
            index is int i ? Payload.ElementAt(i) : throw new ArgumentException("Int must be used as an index");
    }

    public class TagLongArray : ContainerTag<IEnumerable<TagLong>> {
        public override TagType Type => TagType.TagLongArray;

        public override IEnumerable<TagLong> Payload { get; set; }

        public override ITag this[object index] =>
            index is int i ? Payload.ElementAt(i) : throw new ArgumentException("Int must be used as an index");
    }
}