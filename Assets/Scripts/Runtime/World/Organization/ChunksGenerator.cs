using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using Utilities.Math;
using Utilities.Threading.Extensions;
using World.Data;
using Zenject;

namespace World.Organization
{
    public class ChunksGenerator
    {
        private Transform _chunksRoot;
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

        private List<IChunk> _createdChunks = new List<IChunk>();
        private List<ChunkData> _chunksData = new List<ChunkData>();
        private List<Task> _tasks = new List<Task>();

        [Inject]
        public void Construct(DiContainer diContainer,
            BasicChunkSettings basicChunkSettings,
            ChunkCoordinatesCalculator chunkCoordinatesCalculator,
            Transform chunksRoot,
            IChunksContainer chunksContainer,
            IChunkDataProvider chunkDataProvider,
            IChunk chunkPrefab)
        {
            _diContainer = diContainer;
            _basicChunkSettings = basicChunkSettings;
            _activeChunksContainer = chunksContainer;
            _chunksDataProvider = chunkDataProvider;
            _chunksRoot = chunksRoot;

            _chunkPrefabGO = (chunkPrefab as Component)?.gameObject;
            if (_chunkPrefabGO == null)
            {
                throw new ArgumentException($"{nameof(chunkPrefab)} should be a gameObject!");
            }

            _chunkSize = _basicChunkSettings.Size;
            _chunkSizePlusOne = _basicChunkSettings.SizePlusOne;
            _chunkCoordinatesCalculator = chunkCoordinatesCalculator;
        }

        public async Task GenerateChunks(
            IEnumerable<Vector3Int> generatingChunksLocalPositions, 
            int batchLength)
        {
            NativeArray<Vector3Int> batchArray = new NativeArray<Vector3Int>(
                batchLength, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            int batchStamp = 0;
            int counter = 0;
            foreach (var pos in generatingChunksLocalPositions)
            {
                var indexInArray = counter % batchLength;
                batchArray[indexInArray] = pos;

                counter++;
                if (counter % batchLength == 0)
                {
                    batchStamp = counter;
                    await GenerateBatch(batchArray).OnException((ex) => { 
                        Debug.LogException(ex); batchArray.Dispose(); 
                    });
                }
            }

            if(batchStamp != counter)
            {
                var subArray = batchArray.GetSubArray(0, counter - batchStamp);
                await GenerateBatch(subArray).OnException((ex) => {
                    Debug.LogException(ex); batchArray.Dispose();
                });
            }

            batchArray.Dispose();
        }

        private async Task GenerateBatch(NativeArray<Vector3Int> chunkPositionsArray)
        {
            IReadOnlyList<IChunk> chunksGameObjects
                = CreateChunksGameObject(chunkPositionsArray);

            IReadOnlyList<ChunkData> chunksData
                = await GenerateChunksData(chunkPositionsArray);

            await GenerateChunksMeshes(chunkPositionsArray, chunksGameObjects, chunksData);

            ApplyChunksMeshData(chunkPositionsArray, chunksGameObjects);
        }

        private IReadOnlyList<IChunk> CreateChunksGameObject(
            NativeArray<Vector3Int> generatingChunksLocalPositions)
        {
            _createdChunks.Clear();

            foreach (var loadingPos in generatingChunksLocalPositions)
            {
                var pos = loadingPos;
                GameObject instance = _diContainer.InstantiatePrefab(_chunkPrefabGO, _chunksRoot);
                IChunk chunk = instance.GetComponent(typeof(IChunk)) as IChunk;
                _createdChunks.Add(chunk);
                _chunksList.Add(new(chunk, instance));

                if (!_activeChunksContainer.IsChunkExist(pos.x, pos.y, pos.z))
                {
                    _activeChunksContainer.AddChunk(pos.x, pos.y, pos.z, chunk);
                }
                else
                {
                    throw new Exception("HASH COLLISSION!");
                }
            }
            return _createdChunks;
        }

        private async Task<IReadOnlyList<ChunkData>> GenerateChunksData(
            NativeArray<Vector3Int> generatingChunksLocalPositions)
        {
            _chunksData.Clear();

            NativeList<ThreedimensionalNativeArray<VoxelData>> chunksRawData
                = await _chunksDataProvider.GenerateChunksRawData(
                    generatingChunksLocalPositions, _chunkSize, _chunkSizePlusOne);

            try
            {
                for (int i = 0; i < generatingChunksLocalPositions.Length; i++)
                {
                    _chunksData.Add(new ChunkData(chunksRawData[i]));
                }
            }
            finally
            {
                chunksRawData.Dispose();
            }

            return _chunksData;
        }

        private async Task GenerateChunksMeshes(
            NativeArray<Vector3Int> generatingChunksLocalPositions,
            IReadOnlyList<IChunk> chunks,
            IReadOnlyList<ChunkData> chunksData)
        {
            int counter = 0;
            _tasks.Clear();

            foreach (var loadingPos in generatingChunksLocalPositions)
            {
                var pos = loadingPos;
                IChunk chunk = chunks[counter];
                ChunkData chunkData = chunksData[counter];

                chunk.InitializeBasicData(
                        _chunksDataProvider.MaterialAssociations,
                        pos,
                        chunkData);

                _tasks.Add(chunk.GenerateNewMeshData());
                counter++;
            }

            await Task.WhenAll(_tasks);
        }

        private void ApplyChunksMeshData(
            NativeArray<Vector3Int> generatingChunksLocalPositions, 
            IReadOnlyList<IChunk> chunks)
        {
            int counter = 0;
            foreach (var loadingPos in generatingChunksLocalPositions)
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
    }
}
