using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MCMapExport.NBT.Tags {
    public abstract class ContainerTag<T, TValue> : ITag, IEnumerable<TValue> where T : IEnumerable<TValue> {
        public string Name { get; set; }
        public abstract TagType Type { get; }

        public abstract T Payload { get; set; }

        public abstract ITag this[object index] { get; }

        public object PayloadGeneric => Payload;

        public long Depth { get; set; }

        public IEnumerator<TValue> GetEnumerator() {
            return Payload.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public TGet Get<TGet>(object index) {
            return (TGet) this[index];
        }
    }


    public class CompoundTag : ContainerTag<IDictionary<string, ITag>, KeyValuePair<string, ITag>> {
        public override TagType Type => TagType.TagCompound;

        public override IDictionary<string, ITag> Payload { get; set; }

        public bool Contains(string key) {
            return Payload.ContainsKey(key);
        }

        public override ITag this[object index] => index is string i
            ? Payload[i]
            : throw new ArgumentException("String must be used when accessing a dictionary");
    }

    public class ByteArrayTag : ContainerTag<IEnumerable<ByteTag>, ByteTag> {
        public override TagType Type => TagType.TagByteArray;

        public override IEnumerable<ByteTag> Payload { get; set; }

        public override ITag this[object index] =>
            index is int i ? Payload.ElementAt(i) : throw new ArgumentException("Int must be used as an index");
    }


    public class ListTag : ContainerTag<IEnumerable<ITag>, ITag> {
        public override TagType Type => TagType.TagList;
        public override IEnumerable<ITag> Payload { get; set; }

        public IEnumerable<T> ItemsAs<T>() {
            return Payload.Select(x => (T) x);
        }

        public override ITag this[object index] =>
            index is int i ? Payload.ElementAt(i) : throw new ArgumentException("Int must be used as an index");

        public TagType ListType { get; set; }
    }

    public class IntArrayTag : ContainerTag<IEnumerable<IntTag>, IntTag> {
        public override TagType Type => TagType.TagIntArray;

        public override IEnumerable<IntTag> Payload { get; set; }

        public override ITag this[object index] =>
            index is int i ? Payload.ElementAt(i) : throw new ArgumentException("Int must be used as an index");
    }

    public class LongArrayTag : ContainerTag<IEnumerable<LongTag>, LongTag> {
        public override TagType Type => TagType.TagLongArray;

        public override IEnumerable<LongTag> Payload { get; set; }

        public override ITag this[object index] =>
            index is int i ? Payload.ElementAt(i) : throw new ArgumentException("Int must be used as an index");
    }
}