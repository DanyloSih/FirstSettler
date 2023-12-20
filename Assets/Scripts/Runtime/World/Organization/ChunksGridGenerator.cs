using UnityEngine;
using SimpleHeirs;
using System.Collections.Generic;
using System;
using World.Data;

namespace World.Organization
{
    public class ChunksGridGenerator : MonoBehaviour
    {
        [SerializeField] private Transform _chunksRoot;
        [SerializeField] private HeirsProvider<IChunksContainer> _activeChunksContainerHeir;
        [SerializeField] private HeirsProvider<IChunksDataProvider> _chunksDataProviderHeir;
        [SerializeField] private HeirsProvider<IChunk> _chunkPrefabHeir;
        [SerializeField] private BasicChunkSettings _basicChunkSettings;
        [SerializeField] private Vector3Int _chunksGridSize;

        private List<(IChunk ChunkComponent, GameObject ChunkGameObject)> _chunksList
            = new List<(IChunk ChunkComponent, GameObject ChunkGameObject)>();
        private IChunksContainer _activeChunksContainer;
        private GameObject _chunkPrefabGO;
        private IChunksDataProvider _chunksDataProvider;
        private Vector3Int _minPoint;
        private ChunkCoordinatesCalculator _chunkCoordinatesCalculator;
        private MeshDataBuffer _meshDataBuffer;

        public async void OnEnable()
        {
            _chunkCoordinatesCalculator = new ChunkCoordinatesCalculator(_basicChunkSettings.Size, _basicChunkSettings.Scale);

            _activeChunksContainer = _activeChunksContainerHeir.GetValue();
            IChunk chunkPrefab = _chunkPrefabHeir.GetValue();
            _chunkPrefabGO = (chunkPrefab as Component)?.gameObject;
            if (_chunkPrefabGO == null)
            {
                throw new ArgumentException($"{nameof(_chunkPrefabHeir)} should be prefab gameObject!");
            }
            _chunksDataProvider = _chunksDataProviderHeir.GetValue();
            _minPoint = _chunksGridSize / 2;

            Vector3Int size = _basicChunkSettings.Size;
            int cubesCount = size.x * size.y * size.z;
            var meshGenerationAlgorithmInfo 
                = chunkPrefab.MeshGenerationAlgorithm.MeshGenerationAlgorithmInfo;

            _meshDataBuffer = new MeshDataBuffer(
                meshGenerationAlgorithmInfo.MaxVerticesPerMarch * cubesCount,
                meshGenerationAlgorithmInfo.MaxTrianglesPerMarch * cubesCount,
                _chunksDataProvider.MaterialAssociations.GetMaterialKeyHashes());

            DestroyOldChunks();
            InitializeChunks();
        }

        private async void InitializeChunks()
        {
           
            for (int x = -_minPoint.x; x <= _minPoint.x; x++)
            {
                for (int y = -_minPoint.y; y <= _minPoint.y; y++)
                {
                    for (int z = -_minPoint.z; z <= _minPoint.z; z++)
                    {
                        var instance = Instantiate(_chunkPrefabGO, _chunksRoot);
                        IChunk chunk = instance.GetComponent(typeof(IChunk)) as IChunk;
                        _chunksList.Add(new (chunk, instance));
                        var chunkData = await _chunksDataProvider.GetChunkData(x, y, z, _basicChunkSettings.Size);
                        if (!_activeChunksContainer.IsChunkExist(x, y, z))
                        {
                            _activeChunksContainer.AddChunk(x, y, z, chunk);
                        }
                        else
                        {
                            throw new Exception("HASH COLLISSION!");
                        }
                        chunk.InitializeBasicData(
                            _basicChunkSettings,
                            _chunksDataProvider.MaterialAssociations,
                            new Vector3Int(x, y, z),
                            chunkData,
                            _meshDataBuffer);

                        chunk.UpdateMesh();
                        chunk.RootGameObject.transform.localScale
                            = Vector3.one * _basicChunkSettings.Scale;

                        chunk.RootGameObject.transform.position
                            = _chunkCoordinatesCalculator.GetGlobalChunkPositionByLocal(new Vector3Int(x, y, z));
                    }
                }
            }
        }

        private void DestroyOldChunks()
        {
            foreach (var chunk in _chunksList)
            {
                Vector3Int chunkPosition = chunk.ChunkComponent.ChunkPosition;
                _activeChunksContainer.RemoveChunk(chunkPosition.x, chunkPosition.y, chunkPosition.z);
                Destroy(chunk.ChunkGameObject);
            }
        }
    }
}
