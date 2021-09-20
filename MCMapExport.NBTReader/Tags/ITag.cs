namespace MCMapExport.NBT.Tags {
    public interface ITag {
        public ITag this[object index] { get; }

        public string Name { get; }
        object PayloadGeneric { get; }
    }
}