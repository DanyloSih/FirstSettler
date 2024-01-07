using System;
using FirstSettler.Extensions;
using UnityEngine;

namespace World.Data
{
    public class MeshBuffers
    {
        public int MaxVerticesCount;
        public ComputeBuffer VerticesBuffer;
        public ComputeBuffer TrianglesBuffer;
        public ComputeBuffer UVBuffer;
        public ComputeBuffer PolygonsCounter;

        public MeshBuffers(int maxVerticesCount, ComputeBuffer verticesBuffer, ComputeBuffer trianglesBuffer, ComputeBuffer uvsBuffer, ComputeBuffer polygonsCounter)
        {
            MaxVerticesCount = maxVerticesCount;
            VerticesBuffer = verticesBuffer;
            TrianglesBuffer = trianglesBuffer;
            UVBuffer = uvsBuffer;
            PolygonsCounter = polygonsCounter;
        }

        public MeshBuffers(int maxVerticesCount) : 
            this (maxVerticesCount,
                ComputeBufferExtensions.Create(maxVerticesCount, typeof(Vector3)),
                ComputeBufferExtensions.Create(maxVerticesCount, typeof(TriangleAndMaterialHash)),
                ComputeBufferExtensions.Create(maxVerticesCount, typeof(Vector2)),
                ComputeBufferExtensions.Create(maxVerticesCount, typeof(int), ComputeBufferType.Counter))
        {
            
        }

        public void ResetCounters()
        {
            PolygonsCounter.SetCounterValue(0);
        }

        public void DisposeAllBuffers()
        {
            VerticesBuffer.Dispose();
            TrianglesBuffer.Dispose();
            UVBuffer.Dispose();
            PolygonsCounter.Dispose();
        }
    }
}
