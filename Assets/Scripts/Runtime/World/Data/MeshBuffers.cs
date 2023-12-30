using UnityEngine;

namespace World.Data
{
    public class MeshBuffers
    {
        public ComputeBuffer VerticesBuffer;
        public ComputeBuffer TrianglesBuffer;
        public ComputeBuffer UvsBuffer;

        public MeshBuffers(ComputeBuffer verticesBuffer, ComputeBuffer trianglesBuffer, ComputeBuffer uvsBuffer)
        {
            VerticesBuffer = verticesBuffer;
            TrianglesBuffer = trianglesBuffer;
            UvsBuffer = uvsBuffer;
        }
    }
}
