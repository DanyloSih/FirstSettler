using UnityEngine;

namespace World.Data
{
    public class MeshBuffers
    {
        public ComputeBuffer VerticesBuffer;
        public ComputeBuffer TrianglesBuffer;
        public ComputeBuffer UvsBuffer;
        public ComputeBuffer PolygonsCounter;

        public MeshBuffers(ComputeBuffer verticesBuffer, ComputeBuffer trianglesBuffer, ComputeBuffer uvsBuffer, ComputeBuffer polygonsCounter)
        {
            VerticesBuffer = verticesBuffer;
            TrianglesBuffer = trianglesBuffer;
            UvsBuffer = uvsBuffer;
            PolygonsCounter = polygonsCounter;
        }


        public void ResetCounters()
        {
            PolygonsCounter.SetCounterValue(0);
        }
    }
}
