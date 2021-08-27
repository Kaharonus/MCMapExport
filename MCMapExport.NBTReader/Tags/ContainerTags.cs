using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MCMapExport.NBT.Tags {
    public abstract class ContainerTag<T, TValue> : ITag, IEnumerable<TValue> where T : IEnumerable<TValue> {
        public string Name { get; set; }

        public abstract T Payload { get; set; }

        public abstract ITag this[object index] { get; }

        public object PayloadGeneric => Payload;
        
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

        public override IDictionary<string, ITag> Payload { get; set; }

        public bool Contains(string key) {
            return Payload.ContainsKey(key);
        }

        public override ITag this[object index] => index is string i
            ? Payload[i]
            : throw new ArgumentException("String must be used when accessing a dictionary");
    }

    public class ByteArrayTag : ContainerTag<ByteTag[], ByteTag> {

        public override ByteTag[] Payload { get; set; }

        public override ITag this[object index] =>
            index is int i ? Payload.ElementAt(i) : throw new ArgumentException("Int must be used as an index");
    }


    public class ListTag : ContainerTag<ITag[], ITag> {
        public override ITag[] Payload { get; set; }

        public IEnumerable<T> ItemsAs<T>() {
            return Payload.Select(x => (T) x);
        }

        public override ITag this[object index] =>
            index is int i ? Payload.ElementAt(i) : throw new ArgumentException("Int must be used as an index");

        public TagType ListType { get; set; }
    }

    public class IntArrayTag : ContainerTag<IntTag[], IntTag> {
        public override IntTag[] Payload { get; set; }

        public override ITag this[object index] =>
            index is int i ? Payload.ElementAt(i) : throw new ArgumentException("Int must be used as an index");
    }

    public class LongArrayTag : ContainerTag<LongTag[], LongTag> {

        public override LongTag[] Payload { get; set; }

        public override ITag this[object index] =>
            index is int i ? Payload.ElementAt(i) : throw new ArgumentException("Int must be used as an index");
    }
}