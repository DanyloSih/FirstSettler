using UnityEngine;
using SimpleHeirs;
using System.Collections.Generic;
using System;
using World.Data;
using FirstSettler.Extensions;

namespace World.Organization
{

    public class ChunksGridGenerator : MonoBehaviour
    {
        [SerializeField] private Transform _chunksRoot;
        [SerializeField] private HeirsProvider<IChunksContainer> _activeChunksContainerHeir;
        [SerializeField] private HeirsProvider<IChunkDataProvider> _chunksDataProviderHeir;
        [SerializeField] private HeirsProvider<IChunk> _chunkPrefabHeir;
        [SerializeField] private BasicChunkSettings _basicChunkSettings;
        [SerializeField] private Vector3Int _chunksGridSize;

        private List<(IChunk ChunkComponent, GameObject ChunkGameObject)> _chunksList
            = new List<(IChunk ChunkComponent, GameObject ChunkGameObject)>();
        private IChunksContainer _activeChunksContainer;
        private GameObject _chunkPrefabGO;
        private IChunkDataProvider _chunksDataProvider;
        private Vector3Int _minPoint;
        private ChunkCoordinatesCalculator _chunkCoordinatesCalculator;
        private IMatrixWalker _matrixWalker;

        protected void OnEnable()
        {
            _chunkCoordinatesCalculator = new ChunkCoordinatesCalculator(_basicChunkSettings.Size, _basicChunkSettings.Scale);
            _matrixWalker = new SpiralMatrixWalker();
            _activeChunksContainer = _activeChunksContainerHeir.GetValue();
            IChunk chunkPrefab = _chunkPrefabHeir.GetValue();
            _chunkPrefabGO = (chunkPrefab as Component)?.gameObject;
            if (_chunkPrefabGO == null)
            {
                throw new ArgumentException($"{nameof(_chunkPrefabHeir)} should be prefab gameObject!");
            }
            _chunksDataProvider = _chunksDataProviderHeir.GetValue();
            _minPoint = _chunksGridSize / 2;

            DestroyOldChunks();
            InitializeChunks();
        }

        private async void InitializeChunks()
        {
            await _matrixWalker.WalkMatrix(_chunksGridSize, async (x, y, z) => {
                x -= _minPoint.x; 
                y -= _minPoint.y; 
                z -= _minPoint.z;
                Debug.Log($"{x} {y} {z}");
                var instance = Instantiate(_chunkPrefabGO, _chunksRoot);
                IChunk chunk = instance.GetComponent(typeof(IChunk)) as IChunk;
                _chunksList.Add(new(chunk, instance));

                var chunkData = new ChunkData(_basicChunkSettings.Size);
                await _chunksDataProvider.FillChunkData(chunkData, x, y, z);

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
                    chunkData);

                await chunk.GenerateNewMeshData();
                chunk.ApplyMeshData();

                chunk.RootGameObject.transform.localScale
                    = Vector3.one * _basicChunkSettings.Scale;

                chunk.RootGameObject.transform.position
                    = _chunkCoordinatesCalculator.GetGlobalChunkPositionByLocal(new Vector3Int(x, y, z));
            });
        }

        private void DestroyOldChunks()
        {
            foreach (var chunk in _chunksList)
            {
                Vector3Int chunkPosition = chunk.ChunkComponent.LocalPosition;
                _activeChunksContainer.RemoveChunk(chunkPosition.x, chunkPosition.y, chunkPosition.z);
                Destroy(chunk.ChunkGameObject);
            }
        }
    }
}
