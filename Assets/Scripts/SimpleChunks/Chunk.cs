using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using Zenject;
using Utilities.Math;
using SimpleChunks.DataGeneration;
using SimpleChunks.MeshGeneration;

namespace SimpleChunks
{
    public class Chunk : MonoBehaviour, IChunk
    {
        private static VertexAttributeDescriptor s_vertexAttributeDescriptor
            = new VertexAttributeDescriptor(VertexAttribute.Position);

        private static MeshUpdateFlags s_meshUpdateFlags 
            = MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontRecalculateBounds;

        [Inject] private MaterialKeyAndUnityMaterialAssociations _materialAssociations;
        [Inject] private BasicChunkSettings _basicChunkSettings;
        [Inject] private ChunkCoordinatesCalculator _chunkCoordinatesCalculator;

        [SerializeField] private string _chunkNameFormat = "MarchingCubesChunk: x({0}) y({1}) z({2})";
        [SerializeField] private string _meshNameFormat = "Mesh: x({0}) y({1}) z({2})";

        private Bounds _bounds;
        private Vector3Int _localPosition;
        private (MeshFilter MeshFilter, MeshRenderer MeshRenderer, MeshCollider MeshCollider) _meshComponents;
        private Coroutine _updatePhysicsCoroutine;
        private string _meshName;
        private Mesh _cashedMesh;
        private ThreedimensionalNativeArray<VoxelData> _chunkData;

        public Vector3Int LocalPosition { get => _localPosition; }
        public GameObject RootGameObject { get => gameObject; }
        public ThreedimensionalNativeArray<VoxelData> ChunkData { get => _chunkData; }
      
        public void InitializeBasicData(
            Vector3Int chunkLocalPosition,
            ThreedimensionalNativeArray<VoxelData> chunkData)
        {
            _chunkData = chunkData;
            _localPosition = chunkLocalPosition;
            InitializeNames(chunkLocalPosition);
            _bounds = CalculateBounds();

            if (_meshComponents.MeshFilter != null)
            {
                Destroy(_meshComponents.MeshFilter.gameObject);
            }

            _meshComponents = CreateMesh32();
        }

        public void ApplyMeshData(MeshData meshData)
        {
            var isPhysicallyCorrect = meshData.IsPhysicallyCorrect;

            _meshComponents.MeshRenderer.enabled = isPhysicallyCorrect;     

            if (isPhysicallyCorrect)
            {
                _cashedMesh = _meshComponents.MeshFilter.mesh;
                _cashedMesh.Clear();

                ApplyVertices(meshData);
                ApplyTriangles(meshData);

                _cashedMesh.RecalculateNormals(s_meshUpdateFlags);

                _meshComponents.MeshFilter.transform.localPosition = Vector3.zero;
            }

            _meshComponents.MeshCollider.enabled = isPhysicallyCorrect;

            if (_updatePhysicsCoroutine == null)
            {           
                _updatePhysicsCoroutine = StartCoroutine(UpdatePhysicsProcess(isPhysicallyCorrect));
            }
        }

        private void ApplyVertices(MeshData meshData)
        {
            _cashedMesh.SetVertexBufferParams(meshData.VerticesCount, s_vertexAttributeDescriptor);
            _cashedMesh.SetVertexBufferData(
                meshData.Vertices, 
                0, 
                0,
                meshData.VerticesCount, 
                flags: s_meshUpdateFlags);
        }

        private void ApplyTriangles(MeshData meshData)
        {
            _cashedMesh.subMeshCount = meshData.SubmeshesInfo.Length;
            var materials = new Material[meshData.SubmeshesInfo.Length];

            _cashedMesh.SetIndexBufferParams(meshData.IndicesCount, IndexFormat.UInt32);
            _cashedMesh.SetIndexBufferData(meshData.SortedIndices, 0, 0, meshData.IndicesCount, s_meshUpdateFlags);

            int submeshId = 0;
            foreach (var submeshInfo in meshData.SubmeshesInfo)
            {
                materials[submeshId] = _materialAssociations.GetMaterialByKeyHash(submeshInfo.MaterialHash);
                _cashedMesh.SetSubMesh
                    (submeshId,
                    new SubMeshDescriptor(submeshInfo.IndicesStartIndex, submeshInfo.IndicesCount),
                    s_meshUpdateFlags);

                submeshId++;
            }

            _meshComponents.MeshRenderer.materials = materials;
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
            yield return null;
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

        private Bounds CalculateBounds()
        {
            var chunkPos = _chunkCoordinatesCalculator.GetGlobalChunkPositionByLocal(LocalPosition);
            var halfSize = _basicChunkSettings.SizeInVoxels / 2;
            return new Bounds(halfSize, _basicChunkSettings.SizeInVoxels);
        }

        private (MeshFilter, MeshRenderer, MeshCollider) CreateMesh32()
        {
            Mesh mesh = new Mesh();
            mesh.Clear();
            mesh.MarkDynamic();
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.name = _meshName;

            mesh.bounds = _bounds;

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
