using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using MCMapExport.NBT.Tags;

namespace MCMapExport.NBT {
    public class NBTSerializer<T> : NBTDataReader where T : class {
        private static Dictionary<Type, Dictionary<string, PropertyInfo>> _typeCache = new();


        public NBTSerializer(ReadOnlyMemory<byte> bytes, CompressionType type, NBTReaderConfiguration config = null) :
            base(bytes, type, config) {
            BuildTypeCache(typeof(T));
        }

        public NBTSerializer(byte[] bytes, CompressionType type, NBTReaderConfiguration config = null) : base(bytes,
            type, config) {
            BuildTypeCache(typeof(T));
        }

        public NBTSerializer(Stream stream, bool owner, NBTReaderConfiguration config = null) : base(stream, owner,
            config) {
            BuildTypeCache(typeof(T));
        }

        private static bool IsAssignable(object objType, TagType tagType) => (objType, tagType) switch {
            (int, TagType.TagInt) => true,
            (float, TagType.TagFloat) => true,
            (byte, TagType.TagByte) => true,
            (short, TagType.TagShort) => true,
            (double, TagType.TagDouble) => true,
            (long, TagType.TagLong) => true,
            (IDictionary<string, object>, TagType.TagCompound) => true,
            (object, TagType.TagCompound) => true,
            (IList<object>, TagType.TagList) => true,
            (string, TagType.TagString) => true,
            (IList<byte>, TagType.TagByteArray) => true,
            (IList<long>, TagType.TagLongArray) => true,
            (IList<int>, TagType.TagIntArray) => true,
            (_, TagType.TagEnd) => false,
            _ => false
        };

        private void BuildTypeCache(Type type) {
            if (_typeCache.ContainsKey(type)) {
                return;
            }

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(x => x.Name, x => x);
            _typeCache.Add(type, properties);
            foreach (var (name, property) in properties) {
                var propertyType = property.PropertyType;

                foreach (var interfaceType in propertyType.GetInterfaces()) {
                    //Check if its a list or an array type (arrays implement IList)
                    if (!interfaceType.IsGenericType || interfaceType.GetGenericTypeDefinition() != typeof(IList<>)) {
                        continue;
                    }
                    //Check if its an array and get type of array/list
                    var itemType = propertyType.IsArray
                        ? propertyType.GetElementType()
                        : propertyType.GetGenericArguments()[0];
                    //Check if it is class and is not stdlib
                    if (!itemType!.IsClass || itemType.FullName!.StartsWith("System.")) {
                        continue;
                    }

                    BuildTypeCache(itemType);
                    break;
                }

                if (!propertyType.IsClass || propertyType.FullName!.StartsWith("System.") || property.PropertyType.IsArray) {
                    continue;
                }

                BuildTypeCache(propertyType);
            }
        }

        private (string name, object obj) ReadNext<TRead>(TagType tagType, string name){
            
            if (tagType != TagType.TagEnd) {
                name = GetTagName();
            }
        
            switch (tagType) {
                case TagType.TagByte:
                    return (name, ReadByte());
                case TagType.TagShort:
                    return (name ,ReadShort());
                case TagType.TagInt:
                    return (name, ReadInt());
                case TagType.TagLong:
                    return (name, ReadLong());
                case TagType.TagFloat:
                    return (name, ReadFloat());
                case TagType.TagDouble:
                    return (name, ReadDouble());
                case TagType.TagString:
                    return (name, ReadString());
                case TagType.TagList:
                    break;
                case TagType.TagByteArray:
                    break;
                case TagType.TagCompound:
                    return (name, ReadCompound<TRead>());
                    break;
                case TagType.TagIntArray:
                    break;
                case TagType.TagLongArray:
                    break;
                case TagType.TagEnd:
                    return default;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tagType), tagType, null);
            }

            return default;
        }

        private TRead ReadCompound<TRead>() {
            var obj = Activator.CreateInstance<TRead>();
            var type = typeof(TRead);
            var nextTag = GetNextTagType();

            while (nextTag != TagType.TagEnd) {
                var name = GetTagName();
                if (obj is IDictionary<string, object> dict) {
                    obj = (TRead)ReadDictionary();
                }
                else {
                    
                }
                //nextTag = GetNextTagType();
            }
         

            var info = _typeCache[obj.GetType()];

            return obj;
        }

        private IDictionary<string, object> ReadDictionary() {
                var tags = new Dictionary<string, object>();

                return tags;
            
        }
        
        public T Serialize() {
            var type = GetNextTagType();
            return (T)ReadNext<T>(type, "").obj;
        }
    }
}