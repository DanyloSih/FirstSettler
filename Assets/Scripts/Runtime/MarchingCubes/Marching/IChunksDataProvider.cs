namespace MarchingCubesProject
{
    public interface IChunksDataProvider
    {
        public ChunkData GetChunkData(int x, int y, int z);

        public BasicChunkSettings BasicChunkSettings { get; }
        public MaterialKeyAndUnityMaterialAssociations MaterialAssociations { get; }
    }
}
