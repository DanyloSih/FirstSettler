using System.Collections.Generic;
using FirstSettler.Extensions;
using UnityEngine;

namespace World.Data
{
    public class MeshDataBuffersKeeper
    {
        public Vector3[] CashedVertices;
        public TriangleAndMaterialHash[] CashedTriangles;
        public Vector2[] CashedUV;
        private int _maxVerticesCount;
        public int[] ArraysTargetLengths = new int[3];

        private Dictionary<int, List<int>> _materialKeyAndTriangleListAssociations 
            = new Dictionary<int, List<int>>();
        private List<Vector3> _vertsList = new List<Vector3>();
        private MeshBuffers _meshBuffers;

        public int VerticesTargetLength { get => ArraysTargetLengths[0]; set => ArraysTargetLengths[0] = value; }   
        public int TrianglesTargetLength { get => ArraysTargetLengths[1]; set => ArraysTargetLengths[1] = value; }
        public int UvTargetLength { get => ArraysTargetLengths[2]; set => ArraysTargetLengths[2] = value; }

        public MeshDataBuffersKeeper(int maxVerticesCount)
        {
            CashedVertices = new Vector3[maxVerticesCount];
            CashedTriangles = new TriangleAndMaterialHash[maxVerticesCount];
            CashedUV = new Vector2[maxVerticesCount];
            _maxVerticesCount = maxVerticesCount;
        }

        public MeshBuffers GetOrCreateNewMeshBuffers()
        {
            if (_meshBuffers == null)
            {
                _meshBuffers = new MeshBuffers(
                    ComputeBufferExtensions.Create(_maxVerticesCount, typeof(Vector3), ComputeBufferType.Counter, ComputeBufferMode.Immutable),
                    ComputeBufferExtensions.Create(_maxVerticesCount, typeof(TriangleAndMaterialHash), ComputeBufferType.Counter, ComputeBufferMode.Immutable),
                    ComputeBufferExtensions.Create(_maxVerticesCount, typeof(Vector2), ComputeBufferType.Counter, ComputeBufferMode.Immutable));

                //_meshBuffers.VerticesBuffer.SetData(CashedVertices);
                //_meshBuffers.TrianglesBuffer.SetData(CashedTriangles);
                //_meshBuffers.UvsBuffer.SetData(CashedUV);
            }

            return _meshBuffers;  
        }

        public void GetAllDataFromBuffers(MeshBuffers meshBuffers)
        {
            meshBuffers.VerticesBuffer.GetData(CashedVertices);
            meshBuffers.TrianglesBuffer.GetData(CashedTriangles);
            meshBuffers.UvsBuffer.GetData(CashedUV);
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
