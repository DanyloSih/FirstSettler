﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingCubesProject
{
    public class MeshData
    {
        public int VerticesTargetLength;
        public int UvTargetLength;

        public Vector3[] CashedVertices;
        public Vector2[] CashedUV;

        private Dictionary<int, List<int>> _materialKeyAndTriangleListAssociations 
            = new Dictionary<int, List<int>>();

        public MeshData(
            int maxVerticesArrayLength,
            int maxUVArrayLength,
            MaterialKeysPack materialKeysPack)
        {
            CashedVertices = new Vector3[maxVerticesArrayLength];
            CashedUV = new Vector2[maxUVArrayLength];

            _materialKeyAndTriangleListAssociations.Clear();
            foreach (var hash in materialKeysPack.GetMaterialKeyHashes())
            {
                _materialKeyAndTriangleListAssociations.Add(hash, new List<int>());
            }
        }

        public List<int> GetTrianglesListByMaterialKeyHash(int materialKeyHash)
        {
            if (_materialKeyAndTriangleListAssociations.TryGetValue(materialKeyHash, out var triangles))
            {
                return triangles;
            }

            throw new ArgumentException($"There no triangles list associated with hash {materialKeyHash}!");
        }

        public IEnumerable<KeyValuePair<int, List<int>>> GetMaterialKeyHashAndTriangleListAssociations()
            => _materialKeyAndTriangleListAssociations;

        public void ResetAllCollections()
        {
            VerticesTargetLength = 0;
            UvTargetLength = 0;

            foreach (var item in _materialKeyAndTriangleListAssociations)
            {
                item.Value.Clear();
            }
        }
    }
}
