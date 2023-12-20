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
        public int[] ArraysTargetLengths = new int[3];

        private Dictionary<int, List<int>> _materialKeyAndTriangleListAssociations 
            = new Dictionary<int, List<int>>();
        private List<Vector3> _vertsList = new List<Vector3>();

        public int VerticesTargetLength { get => ArraysTargetLengths[0]; set => ArraysTargetLengths[0] = value; }   
        public int TrianglesTargetLength { get => ArraysTargetLengths[1]; set => ArraysTargetLengths[1] = value; }
        public int UvTargetLength { get => ArraysTargetLengths[2]; set => ArraysTargetLengths[2] = value; }
        public ComputeBuffer VerticesBuffer { get; }
        public ComputeBuffer TrianglesBuffer { get; }
        public ComputeBuffer UvsBuffer { get; }
        public ComputeBuffer ArraysTargetLengthBuffer { get; }

        public MeshDataBuffers(int maxVerticesCount)
        {
            CashedVertices = new Vector3[maxVerticesCount];
            CashedTriangles = new TriangleAndMaterialHash[maxVerticesCount];
            CashedUV = new Vector2[maxVerticesCount];
            VerticesBuffer = ComputeBufferExtensions.Create(maxVerticesCount, typeof(Vector3));
            TrianglesBuffer = ComputeBufferExtensions.Create(maxVerticesCount, typeof(TriangleAndMaterialHash));
            UvsBuffer = ComputeBufferExtensions.Create(maxVerticesCount, typeof(Vector2));
            ArraysTargetLengthBuffer = ComputeBufferExtensions.Create(3, typeof(int));
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

        public List<Vector3> GetVertices()
        {
            return _vertsList;
        }

        public void UpdateMeshEssentialsFromCash()
        {
            _materialKeyAndTriangleListAssociations.Clear();
            _vertsList.Clear();
            int i = 0;
            foreach (var triangleInfo in CashedTriangles)
            {
                TriangleAndMaterialHash newInfo = triangleInfo;
                if (newInfo.MaterialHash != 0)
                {
                    _vertsList.Add(CashedVertices[newInfo.Triangle]);
                    newInfo.Triangle = i;
                    i++;

                    if (_materialKeyAndTriangleListAssociations.ContainsKey(newInfo.MaterialHash))
                    {
                        _materialKeyAndTriangleListAssociations[newInfo.MaterialHash].Add(newInfo.Triangle);
                    }
                    else
                    {
                        _materialKeyAndTriangleListAssociations.Add(newInfo.MaterialHash, new List<int>() { newInfo.Triangle });
                    }
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
