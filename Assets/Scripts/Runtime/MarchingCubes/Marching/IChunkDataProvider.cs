namespace MarchingCubesProject
{
    public interface IChunkDataProvider
    {
        public ChunkData GetChunkData(int x, int y, int z, int width, int height, int depth);
    }
}
