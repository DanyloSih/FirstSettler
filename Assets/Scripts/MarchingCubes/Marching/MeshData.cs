using System;
using UnityEngine;

namespace MarchingCubesProject
{
    public class MeshData
    {
        public int VerticesTargetLength;
        public int UvTargetLength;
        public int TrianglesTargetLength;

        private Vector3[] _cashedVertices;
        private Vector2[] _cashedUV;
        private int[] _cashedTriangles;

        public MeshData(int maxVerticesArrayLength, int maxTrianglesArrayLength, int maxUVArrayLength)
        {
            _cashedVertices = new Vector3[maxVerticesArrayLength];
            _cashedTriangles = new int[maxTrianglesArrayLength];
            _cashedUV = new Vector2[maxUVArrayLength];
        }

        public Vector3[] CashedVertices { get => _cashedVertices; }
        public Vector2[] CashedUV { get => _cashedUV; }
        public int[] CashedTriangles { get => _cashedTriangles; }

        public void ResetAllTargetLengths()
        {
            VerticesTargetLength = 0;
            TrianglesTargetLength = 0;
            UvTargetLength = 0;
        }

        public Vector3[] GetCopyOfCashedVerticesWithTargetLength()
        {
            Vector3[] newVertices = new Vector3[VerticesTargetLength];
            Array.Copy(_cashedVertices, newVertices, VerticesTargetLength);
            return newVertices;
        }

        public int[] GetCopyOfCashedTrianglesWithTargetLength()
        {
            int[] newTriangles = new int[TrianglesTargetLength];
            Array.Copy(_cashedTriangles, newTriangles, TrianglesTargetLength);
            return newTriangles;
        }

        public Vector2[] GetCopyOfCashedUVWithTargetLength()
        {
            Vector2[] newUV = new Vector2[UvTargetLength];
            Array.Copy(_cashedUV, newUV, UvTargetLength);
            return newUV;
        }
    }
}
