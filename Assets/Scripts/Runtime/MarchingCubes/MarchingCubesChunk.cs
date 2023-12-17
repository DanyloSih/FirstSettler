﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Linq;
using World.Organization;
using World.Data;
using System.Collections;

namespace MarchingCubesProject
{
    public class MarchingCubesChunk : MonoBehaviour, IChunk
    {
        [SerializeField] private string _chunkNameFormat = "MarchingCubesChunk: x({0}) y({1}) z({2})";
        [SerializeField] private string _meshNameFormat = "Mesh: x({0}) y({1}) z({2})";

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

        public Vector3Int ChunkPosition { get => _chunkPosition; }
        public ChunkData ChunkData { get => _chunkData; }
        public GameObject RootGameObject { get => gameObject; }
        public ChunkNeighbors Neighbors { get; private set; }
        public Vector3Int ChunkSize { get; private set; }

        private bool _isNeighborsInitialized;
        private Coroutine _updatePhysicsCoroutine;
        private List<Material> _currentMaterials;
        private int _filledSubmeshesCount;
        private Renderer _meshRenderer;
        private string _meshName;

        public void InitializeBasicData(
            BasicChunkSettings basicChunkSettings, 
            MaterialKeyAndUnityMaterialAssociations materialKeyAndUnityMaterial, 
            Vector3Int chunkPosition, 
            ChunkData chunkData)
        {

            _chunkPosition = chunkPosition;
            InitializeNames(chunkPosition);

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
            if (_updatePhysicsCoroutine == null)
            {
                _updatePhysicsCoroutine = StartCoroutine(UpdatePhysicsProcess());
            }
        }

        private IEnumerator UpdatePhysicsProcess()
        {
            yield return new WaitForFixedUpdate();

            if (_filledSubmeshesCount > 0)
            {
                _currentMeshComponents.MeshCollider.sharedMesh = null;
                _currentMeshComponents.MeshCollider.sharedMesh
                    = _currentMeshComponents.MeshFilter.sharedMesh;
            }     

            _updatePhysicsCoroutine = null;
        }

        private void InitializeMeshData()
        {
            _currentMaterials = new List<Material>();
            _marching = new MarchingCubesAlgorithm();
            _marching.Surface = 0.0f;
            Vector3Int size = _basicChunkSettings.Size;
            int cubesCount = size.x * size.y * size.z;
            _meshData = new MeshData(
                _marching.MaxVerticesPerMarch * cubesCount,
                _marching.MaxTrianglesPerMarch * cubesCount,
                _materialAssociations.GetMaterialKeyHashes());
        }

        private void InitializeNames(Vector3Int chunkPosition)
        {
            name = string.Format(
                _chunkNameFormat,
                chunkPosition.x.ToString(),
                chunkPosition.y.ToString(),
                chunkPosition.z.ToString());

            _meshName = string.Format(
                _meshNameFormat,
                chunkPosition.x.ToString(),
                chunkPosition.y.ToString(),
                chunkPosition.z.ToString());
        }

        private void InitializeTriangles(MeshData meshData, Mesh mesh)
        {
            mesh.subMeshCount = meshData.GetMaterialKeyHashAndTriangleListAssociations()
                .Where(x => x.Value.Count > 2).Count();
            _currentMaterials.Clear();
            _filledSubmeshesCount = 0;
            int submeshOffset = 0;
            foreach (var item in meshData.GetMaterialKeyHashAndTriangleListAssociations())
            {
                List<int> triangles = item.Value;
                if (triangles.Count < 3)
                {
                    submeshOffset--;
                    continue;
                }
                _currentMaterials.Add(_materialAssociations.GetMaterialByKeyHash(item.Key));
                _filledSubmeshesCount++;
                int minTriangleValue = triangles.Min();
                if (_materialKeyAndSubmeshAssociation.TryGetValue(item.Key, out var submeshId))
                {
                    mesh.SetTriangles(triangles, 0, triangles.Count, submeshId + submeshOffset);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"There no {nameof(submeshId)} associated with hash: {item.Key}");
                }
            }

            _meshRenderer.materials = _currentMaterials.ToArray();
        }

        private (MeshFilter, MeshCollider) CreateMesh32()
        {
            _materialKeyAndSubmeshAssociation.Clear();
            int submeshCounter = 0;
            foreach (var item in _materialAssociations.GetMaterialKeyHashAndMaterialAssociations())
            {
                _materialKeyAndSubmeshAssociation.Add(item.Key, submeshCounter);
                submeshCounter++;
            }

            Mesh mesh = new Mesh();
            mesh.Clear();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.name = _meshName;
            GameObject go = new GameObject(_meshName);
            go.transform.parent = transform;
            var filter = go.AddComponent<MeshFilter>();
            var meshCollider = go.AddComponent<MeshCollider>();
            _meshRenderer = go.AddComponent<MeshRenderer>();
            filter.mesh = mesh;

            return (filter, meshCollider);
        }
    }
}
