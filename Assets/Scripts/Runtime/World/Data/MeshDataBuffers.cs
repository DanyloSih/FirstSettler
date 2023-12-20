using System;
using System.Collections.Generic;
using FirstSettler.Extensions;
using UnityEngine;

namespace World.Data
{
    public class MeshDataBuffers
    {
        public Vector3[] CashedVertices;
        public TriangleAndMaterialHash[] CashedTriangles;
        public Vector2[] CashedUV;
        public int[] ArraysTargetLengths;

        private Dictionary<int, List<int>> _materialKeyAndTriangleListAssociations 
            = new Dictionary<int, List<int>>();

        public int VerticesTargetLength { get => ArraysTargetLengths[0]; set => ArraysTargetLengths[0] = value; }   
        public int TrianglesTargetLength { get => ArraysTargetLengths[1]; set => ArraysTargetLengths[1] = value; }
        public int UvTargetLength { get => ArraysTargetLengths[2]; set => ArraysTargetLengths[2] = value; }
        public ComputeBuffer VerticesBuffer { get; }
        public ComputeBuffer TrianglesBuffer { get; }
        public ComputeBuffer UvsBuffer { get; }
        public ComputeBuffer ArraysTargetLengthBuffer { get; }
        public ComputeBuffer CubesBuffer { get; }
        public ComputeBuffer EdgeVertexBuffer { get; }

        public MeshDataBuffers(int maxVerticesCount, int cubesCount)
        {
            CashedVertices = new Vector3[maxVerticesCount];
            CashedTriangles = new TriangleAndMaterialHash[VerticesTargetLength];
            CashedUV = new Vector2[maxVerticesCount];
            ArraysTargetLengths = new int[3];
            VerticesBuffer = ComputeBufferExtensions.Create(maxVerticesCount, typeof(Vector3));
            TrianglesBuffer = ComputeBufferExtensions.Create(maxVerticesCount, typeof(TriangleAndMaterialHash));
            UvsBuffer = ComputeBufferExtensions.Create(maxVerticesCount, typeof(Vector2));
            ArraysTargetLengthBuffer = ComputeBufferExtensions.Create(3, typeof(int));
            CubesBuffer = ComputeBufferExtensions.Create(cubesCount * 8, typeof(float));
            EdgeVertexBuffer = ComputeBufferExtensions.Create(cubesCount * 12, typeof(Vector3));
            SetAllDataToBuffers();
        }

        public void GetAllDataFromBuffers()
        {
            VerticesBuffer.GetData(CashedVertices);
            TrianglesBuffer.GetData(CashedTriangles);
            UvsBuffer.GetData(CashedUV);
            ArraysTargetLengthBuffer.GetData(ArraysTargetLengths);
        }

        public void SetAllDataToBuffers()
        {
            VerticesBuffer.SetData(CashedVertices);
            TrianglesBuffer.SetData(CashedTriangles);
            UvsBuffer.SetData(CashedUV);
            ArraysTargetLengthBuffer.SetData(ArraysTargetLengths);
        }

        public void UpdateTriangleAssociatoins()
        {
            _materialKeyAndTriangleListAssociations.Clear();
            for (int i = 0; i < TrianglesTargetLength; i++)
            {
                TriangleAndMaterialHash triangleInfo = CashedTriangles[i];
                if (_materialKeyAndTriangleListAssociations.ContainsKey(triangleInfo.MaterialHash))
                {
                    _materialKeyAndTriangleListAssociations[triangleInfo.MaterialHash].Add(triangleInfo.Triangle);
                }
                else
                {
                    _materialKeyAndTriangleListAssociations.Add(triangleInfo.MaterialHash, new List<int>() { triangleInfo.Triangle });
                }
            }
        }

        public IEnumerable<KeyValuePair<int, List<int>>> GetMaterialKeyHashAndTriangleListAssociations()
            => _materialKeyAndTriangleListAssociations;

        public void ResetAllCollections()
        {
            VerticesTargetLength = 0;
            TrianglesTargetLength = 0;
            UvTargetLength = 0;
            _materialKeyAndTriangleListAssociations.Clear();
        }
    }
}
