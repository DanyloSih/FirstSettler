using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.Math;
using World.Data;
using Zenject;

namespace World.Organization
{
    public abstract class ChunksGeneratorBase : MonoBehaviour
    {
        [SerializeField] private Transform _chunksRoot;
        [Tooltip("Determines the volume of the loaded zone in one pass. The value is indicated in chunks.")]
        [SerializeField] private Vector3Int _chunksLoadingVolumePerCall = Vector3Int.one * 2;
      
        private BasicChunkSettings _basicChunkSettings;
        private List<(IChunk ChunkComponent, GameObject ChunkGameObject)> _chunksList
            = new List<(IChunk ChunkComponent, GameObject ChunkGameObject)>();
        private GameObject _chunkPrefabGO;
        private IChunksContainer _activeChunksContainer;
        private IChunkDataProvider _chunksDataProvider;           
        private ChunkCoordinatesCalculator _chunkCoordinatesCalculator;
        private DiContainer _diContainer;
        private Vector3Int _chunkSize;
        private Vector3Int _chunkSizePlusOne;

        protected Vector3Int ChunksLoadingVolumePerCall { get => _chunksLoadingVolumePerCall; }
        protected Transform ChunksRoot { get => _chunksRoot; }
        protected BasicChunkSettings BasicChunkSettings { get => _basicChunkSettings; }
        protected GameObject ChunkPrefabGO { get => _chunkPrefabGO; }
        protected IChunksContainer ActiveChunksContainer { get => _activeChunksContainer; }
        protected IChunkDataProvider ChunksDataProvider { get => _chunksDataProvider; }
        protected DiContainer DiContainer { get => _diContainer; }
        protected Vector3Int ChunkSize { get => _chunkSize; }
        protected Vector3Int ChunkSizePlusOne { get => _chunkSizePlusOne; }
        protected ChunkCoordinatesCalculator ChunkCoordinatesCalculator { get => _chunkCoordinatesCalculator; }

        [Inject]
        public void Construct(DiContainer diContainer,
            BasicChunkSettings basicChunkSettings,
            IChunksContainer chunksContainer,
            IChunkDataProvider chunkDataProvider,
            IChunk chunkPrefab)
        {
            _diContainer = diContainer;
            _basicChunkSettings = basicChunkSettings;
            _activeChunksContainer = chunksContainer;
            _chunksDataProvider = chunkDataProvider;
            
            _chunkPrefabGO = (chunkPrefab as Component)?.gameObject;
            if (_chunkPrefabGO == null)
            {
                throw new ArgumentException($"{nameof(chunkPrefab)} should be a gameObject!");
            }

            _chunkSize = _basicChunkSettings.Size;
            _chunkSizePlusOne = _basicChunkSettings.SizePlusOne;
            _chunkCoordinatesCalculator = new ChunkCoordinatesCalculator(_chunkSize, _basicChunkSettings.Scale);
        }

        protected void OnEnable()
        {
            DestroyOldChunks();
            InitializeChunks();
        }

        protected async Task GenerateChunksBatch(Vector3Int minPos, Vector3Int maxPos)
        {
            RectPrismAreaInt loadingArea = new RectPrismAreaInt(minPos, maxPos);

            Task<List<ChunkData>> GenerateChunksDataTask = GenerateChunksData(loadingArea);

            var chunks = CreateChunksGameObject(loadingArea);

            List<ChunkData> chunksData = await GenerateChunksDataTask;
            //chunksData.ForEach(x => x.Dispose());
            await GenerateChunksMeshes(loadingArea, chunks, chunksData);
            ApplyChunksMeshData(loadingArea, chunks);
        }

        protected abstract void InitializeChunks();

        private void ApplyChunksMeshData(RectPrismAreaInt loadingArea, List<IChunk> chunks)
        {
            int counter = 0;
            foreach (var loadingPos in loadingArea.GetEveryVoxel())
            {
                var chunk = chunks[counter];

                chunk.ApplyMeshData();
                chunk.RootGameObject.transform.localScale
                       = Vector3.one * _basicChunkSettings.Scale;

                chunk.RootGameObject.transform.position
                    = _chunkCoordinatesCalculator.GetGlobalChunkPositionByLocal(loadingPos);

                counter++;
            }
        }

        private async Task GenerateChunksMeshes(RectPrismAreaInt loadingArea, List<IChunk> chunks, List<ChunkData> chunksData)
        {
            int counter = 0;
            List<Task> tasks = new List<Task>();
            foreach (var loadingPos in loadingArea.GetEveryVoxel())
            {
                IChunk chunk = chunks[counter];
                ChunkData chunkData = chunksData[counter];

                chunk.InitializeBasicData(
                        _chunksDataProvider.MaterialAssociations,
                        loadingPos,
                        chunkData);

                tasks.Add(chunk.GenerateNewMeshData());
                counter++;
            }

            await Task.WhenAll(tasks);
        }

        private List<IChunk> CreateChunksGameObject(RectPrismAreaInt loadingArea)
        {
            List<IChunk> createdChunks = new List<IChunk>();
            foreach (var loadingPos in loadingArea.GetEveryVoxel())
            {
                GameObject instance = _diContainer.InstantiatePrefab(_chunkPrefabGO, _chunksRoot);
                IChunk chunk = instance.GetComponent(typeof(IChunk)) as IChunk;
                createdChunks.Add(chunk);
                _chunksList.Add(new(chunk, instance));

                if (!_activeChunksContainer.IsChunkExist(loadingPos.x, loadingPos.y, loadingPos.z))
                {
                    _activeChunksContainer.AddChunk(loadingPos.x, loadingPos.y, loadingPos.z, chunk);
                }
                else
                {
                    throw new Exception("HASH COLLISSION!");
                }
            }
            return createdChunks;
        }

        private async Task<List<ChunkData>> GenerateChunksData(RectPrismAreaInt loadingArea)
        {
            var chunksRawData = await _chunksDataProvider.GenerateChunksRawData(
                loadingArea, _chunkSize, _chunkSizePlusOne);

            List<ChunkData> chunksData = new List<ChunkData>();
            foreach (var index in loadingArea.RectPrism.GetEveryIndex())
            {
                chunksData.Add(new ChunkData(chunksRawData[index]));
            }

            return chunksData;
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
