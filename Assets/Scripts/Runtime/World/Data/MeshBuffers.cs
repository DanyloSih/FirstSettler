using UnityEngine;

namespace World.Data
{
    public class MeshBuffers
    {
        public ComputeBuffer VerticesBuffer;
        public ComputeBuffer TrianglesBuffer;
        public ComputeBuffer UVBuffer;
        public ComputeBuffer PolygonsCounter;

        public MeshBuffers(ComputeBuffer verticesBuffer, ComputeBuffer trianglesBuffer, ComputeBuffer uvsBuffer, ComputeBuffer polygonsCounter)
        {
            VerticesBuffer = verticesBuffer;
            TrianglesBuffer = trianglesBuffer;
            UVBuffer = uvsBuffer;
            PolygonsCounter = polygonsCounter;
        }


        public void ResetCounters()
        {
            PolygonsCounter.SetCounterValue(0);
        }
    }
}
