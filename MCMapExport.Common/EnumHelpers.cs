using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using MCMapExport.Common.Enums;

namespace MCMapExport.Common {
    public static class EnumHelpers {
        private static Dictionary<string, BlockType> _blockTypeCache;

        private static string ToSnakeCase(string text) {
            if (text == null) {
                throw new ArgumentNullException(nameof(text));
            }

            if (text.Length < 2) {
                return text;
            }

            var sb = new StringBuilder();
            sb.Append(char.ToLowerInvariant(text[0]));
            for (var i = 1; i < text.Length; ++i) {
                var c = text[i];
                if (char.IsUpper(c)) {
                    sb.Append('_');
                    sb.Append(char.ToLowerInvariant(c));
                }
                else {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        public static void BuildBlockTypeCache() {
            var names = Enum.GetNames<BlockType>();
            _blockTypeCache = new Dictionary<string, BlockType>(names.Length);
            foreach (var name in names) {
                var key = "minecraft:" + ToSnakeCase(name);
                _blockTypeCache.Add(key, Enum.Parse<BlockType>(name));
            }
            
        }

        public static BlockType BlockTypeFromName(string name) {
            return _blockTypeCache[name];
        }

        public static Color ColorFromBlockType(BlockType type) {
            return ColorMapping.MapColor(type);
        }
    }
}