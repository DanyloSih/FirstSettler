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
using Zenject;

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
        private (MeshFilter MeshFilter, MeshRenderer MeshRenderer, MeshCollider MeshCollider) _meshComponents;
        private bool _isBasicDataInitialized;
        private Coroutine _updatePhysicsCoroutine;
        private List<Material> _currentMaterials;
        private int _filledSubmeshesCount;
        private string _meshName;
        private MarchingCubesAlgorithm _meshGenerationAlgorithm;
        private Mesh _cashedMesh;
        private MeshDataBuffer _cashedMeshData;
        private bool _isMeshDataApplying = false;

        public Vector3Int LocalPosition { get => _localPosition; }
        public ChunkData ChunkData { get => _chunkData; }
        public GameObject RootGameObject { get => gameObject; }
        

        [Inject]
        public void Construct(BasicChunkSettings basicChunkSettings)
        {
            _basicChunkSettings = basicChunkSettings;
            _meshGenerationAlgorithm = new MarchingCubesAlgorithm(
                _generationAlgorithmInfo, _meshGenerationComputeShader, _basicChunkSettings.Size, 0);
        }

        protected void OnDestroy()
        {
            TryDisposeCahsedMeshData();

            if (ChunkData != null)
            {
                ChunkData.Dispose();
            }

            if (_meshGenerationAlgorithm != null)
            {
                _meshGenerationAlgorithm.Dispose();
            }
        }

        public void InitializeBasicData(
            MaterialKeyAndUnityMaterialAssociations materialKeyAndUnityMaterial, 
            Vector3Int chunkPosition, 
            ChunkData chunkData)
        {
            _chunkData = chunkData;
            _localPosition = chunkPosition;
            InitializeNames(chunkPosition);

            _materialAssociations = materialKeyAndUnityMaterial;

            InitializeMeshData();

            if (_meshComponents.MeshFilter != null)
            {
                Destroy(_meshComponents.MeshFilter.gameObject);
            }

            _meshComponents = CreateMesh32();
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

            TryDisposeCahsedMeshData();    
            _cashedMeshData = await _meshGenerationAlgorithm.GenerateMeshData(_chunkData);
        }

        public void ApplyMeshData()
        {
            if (_cashedMeshData == null)
            {
                throw new InvalidOperationException(
                    $"Before using method {nameof(ApplyMeshData)} you " +
                    $"should invoke {nameof(GenerateNewMeshData)} method!");
            }

            var isVerticesCorrect = IsVerticesCorrect();

            _meshComponents.MeshRenderer.enabled = isVerticesCorrect;     

            if (isVerticesCorrect)
            {
                _isMeshDataApplying = true;
                _cashedMesh = _meshComponents.MeshFilter.mesh;
                _cashedMesh.Clear();
                int verticesCount = _cashedMeshData.VerticesCash.Length;
                ApplyVertices(_cashedMeshData.VerticesCash, _cashedMeshData.VerticesCount);
                ApplyTriangles(_cashedMeshData.TrianglesCash, _cashedMeshData.VerticesCount);
                ApplyUVs(_cashedMeshData.UVsCash, _cashedMeshData.VerticesCount);

                _cashedMesh.Optimize();
                _cashedMesh.RecalculateNormals(MeshUpdateFlags.DontResetBoneBounds);

                _meshComponents.MeshFilter.transform.localPosition = Vector3.zero;
            }

            bool isPhysicsCorrect = isVerticesCorrect && IsBoundsNotFlat(_cashedMesh.bounds.size);
            _meshComponents.MeshCollider.enabled = isPhysicsCorrect;

            if (_updatePhysicsCoroutine == null)
            {           
                _updatePhysicsCoroutine = StartCoroutine(UpdatePhysicsProcess(isPhysicsCorrect));
            }

            TryDisposeCahsedMeshData();
        }

        public bool IsMeshDataApplying()
        {
            return _isMeshDataApplying;
        }

        private bool IsBoundsNotFlat(Vector3 boundsSize)
        {
            int notFlatCounter = 0;
            for (int i = 0; i < 3; i++)
            {
                if (boundsSize[i] != 0)
                {
                    notFlatCounter++;
                }
            }
            return notFlatCounter > 1;
        }

        private bool IsVerticesCorrect()
        {
            if (_cashedMeshData.VerticesCount > 2 && _cashedMeshData.VerticesCount % 3 == 0)
            {
                int batchesCount = _cashedMeshData.VerticesCount / 3;
                

                for (int batch = 0; batch < batchesCount; batch++)
                {
                    int startId = batch * 3;
                    var vertex = _cashedMeshData.VerticesCash[startId];
                    int unequalPointCounter = 0;

                    for (int i = startId + 1; i < _cashedMeshData.VerticesCount; i++)
                    {
                        if (vertex != _cashedMeshData.VerticesCash[i])
                        {
                            unequalPointCounter++;
                            if (unequalPointCounter >= 2)
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            return false;
        }

        private void TryDisposeCahsedMeshData()
        {
            if (_cashedMeshData != null)
            {
                _cashedMeshData.DisposeAllArrays();
                _cashedMeshData = null;
            }
        }

        private void ApplyVertices(NativeArray<Vector3> vertices, int verticesCount)
        {
            if (verticesCount == 0)
            {
                return;
            }

            _cashedMesh.SetVertices(vertices, 0, verticesCount);
        }

        private void ApplyTriangles(NativeArray<TriangleAndMaterialHash> nativeTriangles, int verticesCount)
        {
            if (verticesCount == 0)
            {
                return;
            }

            var associations = CalculateMaterialKeyHashAndTriangleListAssociations(nativeTriangles, verticesCount);
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

            _meshComponents.MeshRenderer.materials = _currentMaterials.ToArray();
        }

        private void ApplyUVs(NativeArray<Vector2> uvs, int verticesCount)
        {
            if (verticesCount == 0)
            {
                return;
            }

            _cashedMesh.SetUVs(0, uvs, 0, verticesCount);
        }

        private IEnumerator UpdatePhysicsProcess(bool isPhysicsCorrect)
        {
            yield return new WaitForFixedUpdate();

            if (isPhysicsCorrect)
            {
                if (_meshComponents.MeshCollider.sharedMesh != null)
                {
                    _meshComponents.MeshCollider.sharedMesh = null;
                }

                _meshComponents.MeshCollider.sharedMesh = _cashedMesh;
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
            NativeArray<TriangleAndMaterialHash> triangles, int verticesCount)
        {
            Dictionary<int, List<int>> materialKeyAndTriangleListAssociations
                = new Dictionary<int, List<int>>(_materialAssociations.Count);

            var materials = _materialAssociations.GetMaterialKeyHashes();
            foreach (var item in materials)
            {
                materialKeyAndTriangleListAssociations.Add(item, new List<int>());
            }

            for (int j = 0; j < verticesCount; j++)
            {
                TriangleAndMaterialHash newInfo = triangles[j];
                if (materialKeyAndTriangleListAssociations.ContainsKey(newInfo.MaterialHash))
                {
                    materialKeyAndTriangleListAssociations[newInfo.MaterialHash].Add(newInfo.Triangle);
                }
            }

            return materialKeyAndTriangleListAssociations;
        }

        private (MeshFilter, MeshRenderer, MeshCollider) CreateMesh32()
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
            var meshRenderer = go.AddComponent<MeshRenderer>();
            filter.mesh = mesh;

            return (filter, meshRenderer, meshCollider);
        }
    }
}
