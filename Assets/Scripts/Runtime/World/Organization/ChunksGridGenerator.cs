using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleHeirs;
using UnityEngine;
using Utilities.Math;
using Utilities.Math.Extensions;
using World.Data;
using Zenject;

namespace World.Organization
{
    public class ChunksGridGenerator : MonoBehaviour
    {
        [SerializeField] private Transform _chunksRoot;
        [SerializeField] private HeirsProvider<IChunk> _chunkPrefabHeir;
        [SerializeField] private Vector3Int _chunksGridSize;
        [Tooltip("Determines the volume of the loaded zone in one pass. The value is indicated in chunks.")]
        [SerializeField] private Vector3Int _chunksLoadingVolumePerCall = Vector3Int.one * 2;

        private Vector3Int _loadingGridSize;
        private BasicChunkSettings _basicChunkSettings;
        private List<(IChunk ChunkComponent, GameObject ChunkGameObject)> _chunksList
            = new List<(IChunk ChunkComponent, GameObject ChunkGameObject)>();
        private GameObject _chunkPrefabGO;
        private IChunksContainer _activeChunksContainer;
        private IChunkDataProvider _chunksDataProvider;
        private IMatrixWalker _matrixWalker;
        private Vector3Int _minPoint;
        private ChunkCoordinatesCalculator _chunkCoordinatesCalculator;
        private DiContainer _diContainer;
        private Vector3Int _chunkSize;
        private Vector3Int _chunkSizePlusOne;

        [Inject]
        public void Construct(DiContainer diContainer, 
            BasicChunkSettings basicChunkSettings, 
            IChunksContainer chunksContainer,
            IChunkDataProvider chunkDataProvider)
        {
            _diContainer = diContainer;
            _basicChunkSettings = basicChunkSettings;
            _loadingGridSize = _chunksGridSize.GetElementwiseFloorDividedVector(_chunksLoadingVolumePerCall);
            _chunksGridSize = Vector3Int.Scale(_loadingGridSize, _chunksLoadingVolumePerCall);
            _minPoint = _chunksGridSize / 2;
            _activeChunksContainer = chunksContainer;
            _chunksDataProvider = chunkDataProvider;
        }

        protected void OnEnable()
        {
            _chunkSize = _basicChunkSettings.Size;
            _chunkSizePlusOne = _basicChunkSettings.SizePlusOne;
            _chunkCoordinatesCalculator = new ChunkCoordinatesCalculator(_chunkSize, _basicChunkSettings.Scale);
            _matrixWalker = new SpiralMatrixWalker();
            IChunk chunkPrefab = _chunkPrefabHeir.GetValue();
            _chunkPrefabGO = (chunkPrefab as Component)?.gameObject;
            if (_chunkPrefabGO == null)
            {
                throw new ArgumentException($"{nameof(_chunkPrefabHeir)} should be prefab gameObject!");
            }

            DestroyOldChunks();
            InitializeChunks();
        }

        private async void InitializeChunks()
        {
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            foreach (var batchPos in _matrixWalker.WalkMatrix(_loadingGridSize))
            {
                Vector3Int loadingChunksStartPos = Vector3Int.Scale(batchPos, _chunksLoadingVolumePerCall);
                Vector3Int loadingChunksEndPos = loadingChunksStartPos + _chunksLoadingVolumePerCall;
                await GenerateChunksBatch(loadingChunksStartPos, loadingChunksEndPos);
            }
            stopwatch.Stop();

            Debug.Log($"Chunks loading ended in: {stopwatch.Elapsed.TotalSeconds} seconds");
        }

        private async Task GenerateChunksBatch(Vector3Int minPos, Vector3Int maxPos)
        {
            minPos -= _minPoint;
            maxPos -= _minPoint;
            Area loadingArea = new Area(minPos, maxPos);

            Task<List<ChunkData>> GenerateChunksDataTask = GenerateChunksData(loadingArea);

            var chunks = CreateChunksGameObject(loadingArea);

            List<ChunkData> chunksData = await GenerateChunksDataTask;
            //chunksData.ForEach(x => x.Dispose());
            await GenerateChunksMeshes(loadingArea, chunks, chunksData);
            ApplyChunksMeshData(loadingArea, chunks);
        }

        private void ApplyChunksMeshData(Area loadingArea, List<IChunk> chunks)
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

        private async Task GenerateChunksMeshes(Area loadingArea, List<IChunk> chunks, List<ChunkData> chunksData)
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

        private List<IChunk> CreateChunksGameObject(Area loadingArea)
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

        private async Task<List<ChunkData>> GenerateChunksData(Area loadingArea)
        {
            var chunksRawData = await _chunksDataProvider.GenerateChunksRawData(
                loadingArea, _chunkSize, _chunkSizePlusOne);

            List<ChunkData> chunksData = new List<ChunkData>();
            foreach (var index in loadingArea.RectPrism.GetEveryIndex())
            {
                //var chunk = _chunksDataSource.CopyPart(
                //    loadingPos * _basicChunkSettings.Size, _basicChunkSettings.Size, 32);

                chunksData.Add(new ChunkData(chunksRawData[index]));

                //chunksData.Add(new ChunkData(_basicChunkSettings.Size));
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
