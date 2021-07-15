namespace MCMapExport.Reader.NBT {
    public interface ITag {
            string Name { get; set; }
            TagType Type { get; }
            long Depth { get; set; }
            object PayloadGeneric { get; }
    }
}