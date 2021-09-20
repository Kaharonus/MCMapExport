using System;
using System.Collections;
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

        private static Dictionary<Type, ConstructorInfo> _arrayCache = new();


        private static Dictionary<Type, List<TagType>> _assignable = new() {
            { typeof(int), new() { TagType.TagInt } },
            { typeof(float), new() { TagType.TagFloat } },
            { typeof(byte), new() { TagType.TagByte } },
            { typeof(sbyte), new() { TagType.TagByte } },
            { typeof(short), new() { TagType.TagShort } },
            { typeof(double), new() { TagType.TagDouble } },
            { typeof(long), new() { TagType.TagLong } },
            { typeof(Dictionary<string, object>), new() { TagType.TagCompound } },
            { typeof(List<object>), new() { TagType.TagList } },
            { typeof(string), new() { TagType.TagString } },
            { typeof(List<long>), new() { TagType.TagLongArray } },
            { typeof(List<int>), new() { TagType.TagIntArray } },
            { typeof(List<byte>), new() { TagType.TagByteArray, TagType.TagLongArray, TagType.TagIntArray } },
            { typeof(byte[]), new() { TagType.TagByteArray, TagType.TagLongArray, TagType.TagIntArray } },
            { typeof(long[]), new() { TagType.TagLongArray } },
            { typeof(int[]), new() { TagType.TagIntArray } }
        };

        private TagType _currentType;

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


        private bool IsAssignable(Type type, TagType tag) => _assignable[type].Contains(tag);

        private void AddAssignable(Type type, TagType tag) {
            if (!_assignable.ContainsKey(type)) {
                _assignable.Add(type, new List<TagType>());
            }

            _assignable[type].Add(tag);
        }


        private Type GetTypeOfList(Type list) {
            foreach (var interfaceType in list.GetInterfaces()) {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IList<>)) {
                    var itemType = list.IsArray
                        ? list.GetElementType()
                        : interfaceType.GetGenericArguments()[0];
                    return itemType;
                }
            }

            return default;
        }

        private void BuildTypeCache(Type type) {
            lock (_typeCache) {
                if (_typeCache.ContainsKey(type)) {
                    return;
                }


                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .ToDictionary(x => x.Name, x => x);
                _typeCache.Add(type, properties);
                AddAssignable(type, TagType.TagCompound);
                foreach (var (name, property) in properties) {
                    var propertyType = property.PropertyType;

                    foreach (var interfaceType in propertyType.GetInterfaces()) {
                        //Check if its a list or an array type (arrays implement IList)
                        if (!interfaceType.IsGenericType ||
                            interfaceType.GetGenericTypeDefinition() != typeof(IList<>)) {
                            continue;
                        }

                        //Check if its an array and get type of array/list
                        var itemType = propertyType.IsArray
                            ? propertyType.GetElementType()
                            : propertyType.GetGenericArguments()[0];
                        //Check if it is class and is not stdlib
                        AddAssignable(propertyType, TagType.TagList);
                        if (!itemType!.IsClass || itemType.FullName!.StartsWith("System.")) {
                            continue;
                        }

                        BuildTypeCache(itemType);
                        break;
                    }

                    if (!propertyType.IsClass || propertyType.FullName!.StartsWith("System.") ||
                        property.PropertyType.IsArray) {
                        continue;
                    }

                    BuildTypeCache(propertyType);
                }
            }
        }

        private object ReadNext(Type type, TagType tagType, bool canSkip) {
            _currentType = tagType;
            switch (tagType) {
                case TagType.TagByte:
                    var b = ReadByte();
                    if (type == typeof(sbyte)) {
                        return (sbyte)b;
                    }

                    return b;
                case TagType.TagShort:
                    return ReadShort();
                case TagType.TagInt:
                    return ReadInt();
                case TagType.TagLong:
                    return ReadLong();
                case TagType.TagFloat:
                    return ReadFloat();
                case TagType.TagDouble:
                    return ReadDouble();
                case TagType.TagString:
                    return ReadString();
                case TagType.TagList:
                    return ReadList(type, canSkip);
                case TagType.TagByteArray:
                    return ReadByteArray(type, canSkip);
                case TagType.TagCompound:
                    return ReadCompound(type, canSkip);
                case TagType.TagIntArray:
                    if (type.GetInterfaces().Any(x => x == typeof(IList<byte>))) {
                        return ReadByteArray(type, false);
                    }

                    return ReadIntArray(type, canSkip);
                case TagType.TagLongArray:
                    if (type.GetInterfaces().Any(x => x == typeof(IList<byte>))) {
                        return ReadByteArray(type, false);
                    }

                    return ReadLongArray(type, canSkip);
                case TagType.TagEnd:
                    return default;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tagType), tagType, null);
            }

            return default;
        }

        private PropertyInfo? ReadPropertyInfo(Type type, string name, TagType nextTag) {
            if (name == "BlockStates") {
            }

            var typeInfo = _typeCache[type];
            if (!typeInfo.ContainsKey(name)) {
                return null;
            }

            var property = typeInfo[name];
            if (!IsAssignable(property.PropertyType, nextTag)) {
                return null;
            }

            return property;
        }

        private IList ReadList(Type type, bool canSkip) {
            var tagType = GetNextTagType();
            var tagCount = ReadInt();
            var counter = 0;
            if (canSkip) {
                while (counter < tagCount) {
                    ReadNext(type, tagType, true);
                    counter++;
                }

                return default;
            }

            var data = CreateArray(type, tagCount);
            var baseType = GetTypeOfList(type);
            while (counter < tagCount) {
                SetOrAddValue(data, ReadNext(baseType, tagType, false), counter);
                counter++;
            }

            return data;
        }


        private object ReadCompound(Type type, bool canSkip) {
            var nextTag = GetNextTagType();
            if (canSkip) {
                while (nextTag != TagType.TagEnd) {
                    SkipBytes((ushort)ReadShort()); // Skip the name
                    ReadNext(typeof(Dictionary<string, object>), nextTag, true);
                    nextTag = GetNextTagType();
                }

                return default;
            }
            var obj = Activator.CreateInstance(type);
            while (nextTag != TagType.TagEnd) {
                var name = GetTagName();
                if (obj is Dictionary<string, object> dict) {
                    var value = ReadNext(typeof(Dictionary<string, object>), nextTag, false);
                    dict.Add(name, value);
                } else {
                    var property = ReadPropertyInfo(type, name, nextTag);
                    if (property is null) {
                        ReadNext(typeof(IDictionary<string, object>), nextTag, true);
                    } else {
                        var value = ReadNext(property.PropertyType, nextTag, false);
                        property.SetValue(obj, value);
                    }
                }

                nextTag = GetNextTagType();
            }

            return obj;
        }

        private IList<int> ReadIntArray(Type type, bool canSkip) {
            var count = ReadInt();
            if (canSkip) {
                SkipBytes(count * 4);
                return null;
            }

            var list = CreateArray<int>(type, count);

            for (var i = 0; i < count; i++) {
                SetOrAddValue(list, ReadInt(), i);
            }

            return list;
        }

        private IList<long> ReadLongArray(Type type, bool canSkip) {
            var count = ReadInt();
            if (canSkip) {
                SkipBytes(count * 8);
                return null;
            }

            var list = CreateArray<long>(type, count);
            for (var i = 0; i < count; i++) {
                SetOrAddValue(list, ReadLong(), i);
            }

            return list;
        }

        private IList<byte> ReadByteArray(Type type, bool canSkip) {
            var count = ReadInt();
            if (_currentType == TagType.TagIntArray) {
                count *= 4;
            } else if (_currentType == TagType.TagLongArray) {
                count *= 8;
            }

            if (canSkip) {
                SkipBytes(count);
                return null;
            }

            var list = CreateArray<byte>(type, count);
            for (var i = 0; i < count; i++) {
                SetOrAddValue(list, ReadByte(), i);
            }

            return list;
        }

        private IList<TRead> CreateArray<TRead>(Type type, int size) {
            lock (_arrayCache) {
                if (!_arrayCache.ContainsKey(type)) {
                    var ctor = type.GetConstructors().First(x => {
                        var args = x.GetParameters();
                        return args.Length == 1 && args[0].ParameterType == typeof(int);
                    });
                    _arrayCache.Add(type, ctor);
                }
            }
            return (IList<TRead>)_arrayCache[type].Invoke(new object[] { size });
        }

        private IList CreateArray(Type type, int size) {
            lock (_arrayCache) {
                if (!_arrayCache.ContainsKey(type)) {
                    var ctor = type.GetConstructors().First(x => {
                        var args = x.GetParameters();
                        return args.Length == 1 && args[0].ParameterType == typeof(int);
                    });
                    _arrayCache.Add(type, ctor);
                }
            }
            return (IList)_arrayCache[type].Invoke(new object[] { size });
        }

        private void SetOrAddValue<TValue>(IList<TValue> obj, TValue item, int index = 0) {
            switch (obj) {
                case TValue[] arr:
                    arr[index] = item;
                    break;
                case List<TValue> list:
                    list.Add(item);
                    break;
                default:
                    throw new NotSupportedException($"Type {typeof(TValue).Name} is not supported");
            }
        }

        private void SetOrAddValue(IList list, object item, int index = 0) {
            switch (list) {
                case Array arr:
                    arr.SetValue(item, index);
                    break;
                case { }:
                    list.Add(item);
                    break;
                default:
                    throw new NotSupportedException($"Type {item.GetType().Name} is not supported");
            }
        }


        public T Serialize() {
            var type = GetNextTagType();
            var name = GetTagName();
            return (T)ReadNext(typeof(T), type, false);
        }
    }
}