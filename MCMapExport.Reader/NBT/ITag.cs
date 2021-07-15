namespace MCMapExport.Reader.NBT {
    public interface ITag {
        
            
            public ITag this[object index] { get; }
            
            string Name { get; set; }
            TagType Type { get; }
            long Depth { get; set; }
            object PayloadGeneric { get; }
    }
}