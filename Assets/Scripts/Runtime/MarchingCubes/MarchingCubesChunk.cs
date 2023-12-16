using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Linq;
using World.Organization;
using World.Data;

namespace MarchingCubesProject
{
    public class MarchingCubesChunk : MonoBehaviour, IChunk
    {
        private MaterialKeyAndUnityMaterialAssociations _materialAssociations;
        private BasicChunkSettings _basicChunkSettings;
        private Vector3Int _chunkPosition;
        private Dictionary<int, int> _materialKeyAndSubmeshAssociation = new Dictionary<int, int>();
        private List<Vector3> _normals = new List<Vector3>();
        private MarchingAlgorithm _marching;
        private ChunkData _chunkData;
        private (MeshFilter MeshFilter, MeshCollider MeshCollider) _currentMeshComponents;
        private bool _isBasicDataInitialized;
        private MeshData _meshData;
        private bool _meshColliderUpdated = true;

        public Vector3Int ChunkPosition { get => _chunkPosition; }
        public ChunkData ChunkData { get => _chunkData; }
        public GameObject RootGameObject { get => gameObject; }
        public ChunkNeighbors Neighbors { get; private set; }
        public Vector3Int ChunkSize { get; private set; }

        private bool _isNeighborsInitialized;

        private void FixedUpdate()
        {
            if (!_meshColliderUpdated)
            {
                _currentMeshComponents.MeshCollider.sharedMesh = null;
                _currentMeshComponents.MeshCollider.sharedMesh
                    = _currentMeshComponents.MeshFilter.sharedMesh;
                _meshColliderUpdated = true;
            }
        }

        public void InitializeBasicData(
            BasicChunkSettings basicChunkSettings, 
            MaterialKeyAndUnityMaterialAssociations materialKeyAndUnityMaterial, 
            Vector3Int chunkPosition, 
            ChunkData chunkData)
        {
            _chunkPosition = chunkPosition;
            _materialAssociations = materialKeyAndUnityMaterial;
            _basicChunkSettings = basicChunkSettings;
            ChunkSize = _basicChunkSettings.Size;
            _chunkData = chunkData;

            InitializeMeshData();

            if (_currentMeshComponents.MeshFilter != null)
            {
                Destroy(_currentMeshComponents.MeshFilter.gameObject);
            }

            _currentMeshComponents = CreateMesh32();
            _isBasicDataInitialized = true;
        }

        public void InitializeNeighbors(ChunkNeighbors chunkNeighbors)
        {
            Neighbors = chunkNeighbors;
            _isNeighborsInitialized = true;
        }

        /// <summary>
        /// Updates chunk mesh. Before use this method, you should initialize
        /// chunk using this methods: InitializeBasicData and InitializeNeighbors
        /// </summary>
        public void UpdateMesh()
        {
            if (!_isBasicDataInitialized || !_isNeighborsInitialized)
            {
                throw new InvalidOperationException(
                    $"Before use {nameof(UpdateMesh)} method, you should initialize " +
                    $"chunk using this methods: {nameof(InitializeBasicData)} and {nameof(InitializeNeighbors)}");
            }

            _meshData = _marching.GenerateMeshData(_chunkData, _meshData, Neighbors);
            UpdateMesh(_meshData, _normals);
        }

        private void UpdateMesh(MeshData meshData, List<Vector3> normals)
        {
            var mesh = _currentMeshComponents.MeshFilter.mesh;
            mesh.Clear();
            mesh.SetVertices(meshData.CashedVertices, 0, meshData.VerticesTargetLength);
            InitializeTriangles(meshData, mesh);
            mesh.SetUVs(0, meshData.CashedUV, 0, meshData.UvTargetLength);

            if (normals.Count > 0)
                mesh.SetNormals(normals);
            else
                mesh.RecalculateNormals();

            mesh.RecalculateBounds();

            _currentMeshComponents.MeshFilter.transform.localPosition = Vector3.zero;
            _meshColliderUpdated = false;
        }

        private void InitializeMeshData()
        {
            _marching = new MarchingCubesAlgorithm();
            _marching.Surface = 0.0f;
            Vector3Int size = _basicChunkSettings.Size;
            int cubesCount = size.x * size.y * size.z;
            _meshData = new MeshData(
                _marching.MaxVerticesPerMarch * cubesCount,
                _marching.MaxTrianglesPerMarch * cubesCount,
                _materialAssociations.GetMaterialKeyHashes());
        }

        private void InitializeTriangles(MeshData meshData, Mesh mesh)
        {
            mesh.subMeshCount = meshData.GetMaterialKeyHashAndTriangleListAssociations().Count();
            foreach (var item in meshData.GetMaterialKeyHashAndTriangleListAssociations())
            {
                List<int> triangles = item.Value;
                if (triangles.Count == 0)
                {
                    continue;
                }

                int minTriangleValue = triangles.Min();
                if (_materialKeyAndSubmeshAssociation.TryGetValue(item.Key, out var submeshId))
                {
                    //mesh.SetSubMesh(submeshId, new SubMeshDescriptor(minTriangleValue, triangles.Count));
                    mesh.SetTriangles(triangles, 0, triangles.Count, submeshId);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"There no {nameof(submeshId)} associated with hash: {item.Key}");
                }
            }
        }

        private (MeshFilter, MeshCollider) CreateMesh32()
        {
            List<Material> materials = new List<Material>();
            List<SubMeshDescriptor> descriptors = new List<SubMeshDescriptor>();
            _materialKeyAndSubmeshAssociation.Clear();
            int submeshCounter = 0;
            foreach (var item in _materialAssociations.GetMaterialKeyHashAndMaterialAssociations())
            {
                _materialKeyAndSubmeshAssociation.Add(item.Key, submeshCounter);
                materials.Add(item.Value);
                submeshCounter++;
            }

            Mesh mesh = new Mesh();
            mesh.Clear();
            mesh.subMeshCount = materials.Count;
            mesh.indexFormat = IndexFormat.UInt32;

            GameObject go = new GameObject("Mesh");
            go.transform.parent = transform;
            var filter = go.AddComponent<MeshFilter>();
            var meshCollider = go.AddComponent<MeshCollider>();
            go.AddComponent<MeshRenderer>();
            Renderer renderer = go.GetComponent<Renderer>();
            filter.mesh = mesh;
            filter.mesh.subMeshCount = materials.Count;
            renderer.materials = materials.ToArray();

            return (filter, meshCollider);
        }
    }
}
