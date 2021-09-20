using System;

namespace MCMapExport.NBT.Tags {
    public enum TagType {
        TagEnd = 0,
        TagByte = 1,
        TagShort = 2,
        TagInt = 3,
        TagLong = 4,
        TagFloat = 5,
        TagDouble = 6,
        TagByteArray = 7,
        TagString = 8,
        TagList = 9,
        TagCompound = 10,
        TagIntArray = 11,
        TagLongArray = 12
    }

    public static class Extensions {

        public static bool IsDefined(this TagType type) {
            switch (type) {
                case TagType.TagEnd:
                case TagType.TagByte:
                case TagType.TagShort:
                case TagType.TagInt:
                case TagType.TagLong:
                case TagType.TagFloat:
                case TagType.TagDouble:
                case TagType.TagByteArray:
                case TagType.TagString:
                case TagType.TagList:
                case TagType.TagCompound:
                case TagType.TagIntArray:
                case TagType.TagLongArray:
                    return true;
                default:
                    return false;
            }
        }
        
    }
}