using System;
using UnityEngine;

namespace MarchingCubesProject
{
    public class MeshData
    {
        public int VerticesTargetLength;
        public int UvTargetLength;
        public int TrianglesTargetLength;

        public Vector3[] CashedVertices;
        public Vector2[] CashedUV;
        public int[] CashedTriangles;

        public MeshData(int maxVerticesArrayLength, int maxTrianglesArrayLength, int maxUVArrayLength)
        {
            CashedVertices = new Vector3[maxVerticesArrayLength];
            CashedTriangles = new int[maxTrianglesArrayLength];
            CashedUV = new Vector2[maxUVArrayLength];
        }

        public void ResetAllTargetLengths()
        {
            VerticesTargetLength = 0;
            TrianglesTargetLength = 0;
            UvTargetLength = 0;
        }
    }
}
