namespace SimpleChunks.MeshGeneration
{
    public struct GPUMeshDataFixJobOutput
    {
        public bool IsPhysicallyCorrect;
        public int VerticesCount;

        public GPUMeshDataFixJobOutput(bool isPhysicallyCorrect, int verticesCount)
        {
            IsPhysicallyCorrect = isPhysicallyCorrect;
            VerticesCount = verticesCount;
        }
    }
}
