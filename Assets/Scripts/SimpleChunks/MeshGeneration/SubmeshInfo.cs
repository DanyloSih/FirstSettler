namespace SimpleChunks.MeshGeneration
{
    public struct SubmeshInfo
    {
        public int MaterialHash;
        public int IndicesStartIndex;
        public int IndicesCount;

        public SubmeshInfo(
            int materialHash, 
            int trianglesStartIndex, 
            int trianglesCount)
        {
            MaterialHash = materialHash;
            IndicesStartIndex = trianglesStartIndex;
            IndicesCount = trianglesCount;
        }
    }
}
