using System.Collections.Generic;
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
        [SerializeField] private GenerationAlgorithmInfo _generationAlgorithmInfo;
        [SerializeField] private ComputeShader _meshGenerationComputeShader;

        private MaterialKeyAndUnityMaterialAssociations _materialAssociations;
        private BasicChunkSettings _basicChunkSettings;
        private Vector3Int _chunkPosition;
        private Dictionary<int, int> _materialKeyAndSubmeshAssociation = new Dictionary<int, int>();
        private List<Vector3> _normals = new List<Vector3>();
        private ChunkData _chunkData;
        private (MeshFilter MeshFilter, MeshCollider MeshCollider) _currentMeshComponents;
        private bool _isBasicDataInitialized;
        private MeshDataBuffers _meshDataBuffer;
        private Coroutine _updatePhysicsCoroutine;
        private List<Material> _currentMaterials;
        private int _filledSubmeshesCount;
        private Renderer _meshRenderer;
        private string _meshName;
        private MarchingCubesAlgorithm _marchingCubesAlgorithm;

        public Vector3Int ChunkPosition { get => _chunkPosition; }
        public Vector3Int ChunkSize { get; private set; }
        public ChunkData ChunkData { get => _chunkData; }
        public GameObject RootGameObject { get => gameObject; }
        public IMeshGenerationAlgorithm MeshGenerationAlgorithm
            => _marchingCubesAlgorithm ?? new MarchingCubesAlgorithm(_generationAlgorithmInfo, _meshGenerationComputeShader, 0);

        public void InitializeBasicData(
            BasicChunkSettings basicChunkSettings, 
            MaterialKeyAndUnityMaterialAssociations materialKeyAndUnityMaterial, 
            Vector3Int chunkPosition, 
            ChunkData chunkData,
            MeshDataBuffers meshDataBuffer)
        {
            _chunkData = chunkData;
            _meshDataBuffer = meshDataBuffer;
            _chunkPosition = chunkPosition;
            InitializeNames(chunkPosition);

            _materialAssociations = materialKeyAndUnityMaterial;
            _basicChunkSettings = basicChunkSettings;
            ChunkSize = _basicChunkSettings.Size;

            InitializeMeshData();

            if (_currentMeshComponents.MeshFilter != null)
            {
                Destroy(_currentMeshComponents.MeshFilter.gameObject);
            }

            _currentMeshComponents = CreateMesh32();
            _isBasicDataInitialized = true;
        }

        /// <summary>
        /// Updates chunk mesh. Before use this method, you should initialize
        /// chunk using this methods: InitializeBasicData and InitializeNeighbors
        /// </summary>
        public void UpdateMesh()
        {
            if (!_isBasicDataInitialized)
            {
                throw new InvalidOperationException(
                    $"Before use {nameof(UpdateMesh)} method, you should initialize " +
                    $"chunk using this method: {nameof(InitializeBasicData)}");
            }

            MeshGenerationAlgorithm.GenerateMeshData(_chunkData, _meshDataBuffer);
            UpdateMesh(_meshDataBuffer, _normals);
        }

        private void UpdateMesh(MeshDataBuffers meshData, List<Vector3> normals)
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

            meshData.ResetAllCollections();

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

        private void InitializeTriangles(MeshDataBuffers meshData, Mesh mesh)
        {
            meshData.UpdateTriangleAssociatoins();
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
