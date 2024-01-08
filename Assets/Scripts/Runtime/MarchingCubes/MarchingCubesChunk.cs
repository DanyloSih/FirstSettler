using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Linq;
using World.Organization;
using World.Data;
using System.Collections;
using System.Threading.Tasks;
using Unity.Collections;

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
        private Vector3Int _localPosition;
        private Dictionary<int, int> _materialKeyAndSubmeshAssociation = new Dictionary<int, int>();
        private ChunkData _chunkData;
        private (MeshFilter MeshFilter, MeshCollider MeshCollider) _currentMeshComponents;
        private bool _isBasicDataInitialized;
        private Coroutine _updatePhysicsCoroutine;
        private List<Material> _currentMaterials;
        private int _filledSubmeshesCount;
        private Renderer _meshRenderer;
        private string _meshName;
        private MarchingCubesAlgorithm _marchingCubesAlgorithm;
        private Mesh _cashedMesh;
        private DisposableMeshData _cashedDisposableMeshData;
        private bool _isMeshDataApplying = false;

        public Vector3Int LocalPosition { get => _localPosition; }
        public ChunkData ChunkData { get => _chunkData; }
        public GameObject RootGameObject { get => gameObject; }
        public IMeshGenerationAlgorithm MeshGenerationAlgorithm
            => _marchingCubesAlgorithm ?? new MarchingCubesAlgorithm(
                _generationAlgorithmInfo, _meshGenerationComputeShader, _basicChunkSettings.Size, 0);

        protected void OnDestroy()
        {
            if (ChunkData != null)
            {
                ChunkData.VoxelsData.GetOrCreateVoxelsDataBuffer().Dispose();
            }

            if (_cashedDisposableMeshData != null)
            {
                _cashedDisposableMeshData.DisposeAllArrays();
            }
        }

        public void InitializeBasicData(
            BasicChunkSettings basicChunkSettings, 
            MaterialKeyAndUnityMaterialAssociations materialKeyAndUnityMaterial, 
            Vector3Int chunkPosition, 
            ChunkData chunkData)
        {
            _chunkData = chunkData;
            _localPosition = chunkPosition;
            InitializeNames(chunkPosition);

            _materialAssociations = materialKeyAndUnityMaterial;
            _basicChunkSettings = basicChunkSettings;

            InitializeMeshData();

            if (_currentMeshComponents.MeshFilter != null)
            {
                Destroy(_currentMeshComponents.MeshFilter.gameObject);
            }

            _currentMeshComponents = CreateMesh32();
            _isBasicDataInitialized = true;
        }

        public async Task GenerateNewMeshData()
        {
            if (!_isBasicDataInitialized)
            {
                throw new InvalidOperationException(
                    $"Before use {nameof(GenerateNewMeshData)} method, you should initialize " +
                    $"chunk using this method: {nameof(InitializeBasicData)}");
            }

            if (_cashedDisposableMeshData != null)
            {
                _cashedDisposableMeshData.DisposeAllArrays();
                _cashedDisposableMeshData = null;
            }

            _cashedMesh = _currentMeshComponents.MeshFilter.mesh;
            _cashedDisposableMeshData = await MeshGenerationAlgorithm.GenerateMeshData(_chunkData);
        }

        public void ApplyMeshData()
        {
            if (_cashedDisposableMeshData == null)
            {
                throw new InvalidOperationException(
                    $"Before using method {nameof(ApplyMeshData)} you " +
                    $"should invoke {nameof(GenerateNewMeshData)} method!");
            }
            _isMeshDataApplying = true;
            _cashedMesh.Clear();

            ApplyVertices(_cashedDisposableMeshData.VerticesCash);
            ApplyTriangles(_cashedDisposableMeshData.TrianglesCash);
            ApplyUVs(_cashedDisposableMeshData.UVsCash);
            _cashedMesh.Optimize();
            _cashedMesh.RecalculateNormals(MeshUpdateFlags.DontResetBoneBounds);
            _currentMeshComponents.MeshFilter.transform.localPosition = Vector3.zero;
            if (_updatePhysicsCoroutine == null)
            {
                int verticesCount = _cashedDisposableMeshData.VerticesCash.Length;
                //Debug.Log($"Vertices count pre: {verticesCount}");
                _updatePhysicsCoroutine = StartCoroutine(UpdatePhysicsProcess(verticesCount));
            }
            _cashedDisposableMeshData.DisposeAllArrays();
            _cashedDisposableMeshData = null;
        }

        public bool IsMeshDataApplying()
        {
            return _isMeshDataApplying;
        }

        private void ApplyVertices(NativeArray<Vector3> vertices)
        {
            if (vertices.Length == 0)
            {
                return;
            }

            _cashedMesh.SetVertices(vertices);
        }

        private void ApplyTriangles(NativeArray<TriangleAndMaterialHash> nativeTriangles)
        {
            if (nativeTriangles.Length == 0)
            {
                return;
            }

            var associations = CalculateMaterialKeyHashAndTriangleListAssociations(nativeTriangles);
            _cashedMesh.subMeshCount = associations
                .Where(x => x.Value.Count > 2).Count();
            _currentMaterials.Clear();
            _filledSubmeshesCount = 0;
            int submeshOffset = 0;
            foreach (var item in associations)
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
                    _cashedMesh.SetTriangles(triangles, 0, triangles.Count, submeshId + submeshOffset);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"There no {nameof(submeshId)} associated with hash: {item.Key}");
                }
            }

            _meshRenderer.materials = _currentMaterials.ToArray();
        }

        private void ApplyUVs(NativeArray<Vector2> uvs)
        {
            if (uvs.Length == 0)
            {
                return;
            }

            _cashedMesh.SetUVs(0, uvs);
        }

        private IEnumerator UpdatePhysicsProcess(int verticesCount)
        {
            yield return new WaitForEndOfFrame();

            if (_currentMeshComponents.MeshCollider.sharedMesh != null)
            {
                _currentMeshComponents.MeshCollider.sharedMesh = null;
            }

            //Debug.Log($"Vertices count post: {verticesCount}");
            if (verticesCount > 2 && _filledSubmeshesCount > 0)
            {
                _currentMeshComponents.MeshCollider.sharedMesh = _cashedMesh;
            }     

            _updatePhysicsCoroutine = null;
            _isMeshDataApplying = false;
            yield return null;
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

        private Dictionary<int, List<int>> CalculateMaterialKeyHashAndTriangleListAssociations(
            NativeArray<TriangleAndMaterialHash> triangles)
        {
            Dictionary<int, List<int>> materialKeyAndTriangleListAssociations
                = new Dictionary<int, List<int>>(_materialAssociations.Count);

            var materials = _materialAssociations.GetMaterialKeyHashes();
            foreach (var item in materials)
            {
                materialKeyAndTriangleListAssociations.Add(item, new List<int>());
            }

            int trianglesCount = triangles.Length;

            for (int j = 0; j < trianglesCount; j++)
            {
                TriangleAndMaterialHash newInfo = triangles[j];
                if (newInfo.MaterialHash != 0)
                {
                    materialKeyAndTriangleListAssociations[newInfo.MaterialHash].Add(newInfo.Triangle);
                }
            }

            return materialKeyAndTriangleListAssociations;
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
            mesh.MarkDynamic();
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
