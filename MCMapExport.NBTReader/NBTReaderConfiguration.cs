namespace MCMapExport.NBT {
    public class NBTReaderConfiguration {
        /// <summary>
        /// When set to false all long arrays will be interpreted as byte arrays
        /// </summary>
        public bool UseLongArrays { get; init; } = true;

        /// <summary>
        /// When set to false all int arrays will be interpreted as byte arrays
        /// </summary>
        public bool UseIntArrays { get; init; } = true;
    }
}