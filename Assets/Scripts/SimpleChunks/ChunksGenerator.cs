using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SimpleChunks.DataGeneration;
using SimpleChunks.MeshGeneration;
using Unity.Collections;
using UnityEngine;
using Utilities.Common;
using Utilities.Math;
using Utilities.Threading.Extensions;
using Zenject;

namespace SimpleChunks
{
    public class ChunksGenerator
    {
        private Transform _chunksRoot;
        private MeshGenerator _meshGenerator;
        private BasicChunkSettings _basicChunkSettings;
        private GameObject _chunkPrefabGO;
        private ChunksContainer _activeChunksContainer;
        private IChunkDataProvider _chunksDataProvider;
        private SceneStateProvider _sceneStateProvider;
        private ChunkCoordinatesCalculator _chunkCoordinatesCalculator;
        private DiContainer _diContainer;
        private Vector3Int _chunkSize;
        private Vector3Int _chunkSizePlusOne;
        private List<IChunk> _createdChunks = new List<IChunk>();

        [Inject]
        public void Construct(DiContainer diContainer,
            BasicChunkSettings basicChunkSettings,
            ChunkCoordinatesCalculator chunkCoordinatesCalculator,
            Transform chunksRoot,
            MeshGenerator meshGenerator,
            ChunksContainer chunksContainer,
            SceneStateProvider sceneStateProvider,
            IChunkDataProvider chunkDataProvider,
            IChunk chunkPrefab)
        {
            _diContainer = diContainer;
            _basicChunkSettings = basicChunkSettings;
            _activeChunksContainer = chunksContainer;
            _chunksDataProvider = chunkDataProvider;
            _sceneStateProvider = sceneStateProvider;
            _chunksRoot = chunksRoot;
            _meshGenerator = meshGenerator;

            _chunkPrefabGO = (chunkPrefab as Component)?.gameObject;
            if (_chunkPrefabGO == null)
            {
                throw new ArgumentException($"{nameof(chunkPrefab)} should be a gameObject!");
            }

            _chunkSize = _basicChunkSettings.SizeInCubes;
            _chunkSizePlusOne = _basicChunkSettings.SizeInVoxels;
            _chunkCoordinatesCalculator = chunkCoordinatesCalculator;
        }

        public async Task GenerateChunks(
            IEnumerable<Vector3Int> generatingChunksLocalPositions, 
            int batchLength,
            CancellationToken? cancellationToken = null)
        {
            if (cancellationToken.IsCanceled())
            {
                return;
            }

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
                    await GenerateBatch(batchArray, cancellationToken).OnException((ex) => { 
                        Debug.LogException(ex); batchArray.Dispose(); 
                    });

                    if (cancellationToken.IsCanceled())
                    {
                        batchArray.Dispose();
                        return;
                    }
                }
            }

            if(batchStamp != counter)
            {
                NativeArray<Vector3Int> subArray = batchArray.GetSubArray(0, counter - batchStamp);
                await GenerateBatch(subArray, cancellationToken).OnException((ex) => {
                    Debug.LogException(ex); batchArray.Dispose();
                });

                if (cancellationToken.IsCanceled())
                {
                    subArray.Dispose();
                    batchArray.Dispose();
                    return;
                }
            }

            batchArray.Dispose();

            Debug.Log("Chunks generated.");
        }

        private async Task GenerateBatch(
            NativeArray<Vector3Int> chunkPositionsArray, 
            CancellationToken? cancellationToken = null)
        {
            IReadOnlyList<IChunk> chunksGameObjects
                = CreateChunksGameObject(chunkPositionsArray);

            NativeParallelHashMap<long, UnsafeNativeArray<VoxelData>> chunksData
                = await _chunksDataProvider.GenerateChunksRawData(
                    chunkPositionsArray, cancellationToken);

            if (cancellationToken.IsCanceled())
            {
                return;
            }

            InitializeChunks(chunksGameObjects, chunkPositionsArray, chunksData);

            MeshData[] chunksMeshData = await _meshGenerator
                .GenerateMeshDataForChunks(chunkPositionsArray, chunksData.AsReadOnly(), cancellationToken)
                .OnException(ex => Debug.LogException(ex));

            if (!cancellationToken.IsCanceled())
            {
                ApplyChunksMeshData(chunksMeshData, chunksGameObjects);
            }       

            foreach (var meshData in chunksMeshData)
            {
                meshData.Dispose();
            }

            if (chunksData.IsCreated)
            {
                chunksData.Dispose();
            }
        }

        private IReadOnlyList<IChunk> CreateChunksGameObject(
            NativeArray<Vector3Int> generatingChunksLocalPositions)
        {
            _createdChunks.Clear();

            foreach (var loadingPos in generatingChunksLocalPositions)
            {
                GameObject instance = _diContainer.InstantiatePrefab(_chunkPrefabGO, _chunksRoot);
                IChunk chunk = instance.GetComponent(typeof(IChunk)) as IChunk;
                _createdChunks.Add(chunk);
            }
            return _createdChunks;
        }

        private void InitializeChunks(
            IReadOnlyList<IChunk> chunks, 
            NativeArray<Vector3Int> chunksPositions,
            NativeParallelHashMap<long, UnsafeNativeArray<VoxelData>> chunksData)
        {
            var chunkVoxelsSize = _basicChunkSettings.SizeInVoxels;
            int counter = 0;
            foreach (var chunkPosition in chunksPositions)
            {
                chunksData.TryGetValue(PositionLongHasher.GetHashFromPosition(chunkPosition), out var chunkData);

                chunks[counter].InitializeBasicData(
                    chunksPositions[counter], 
                    new (chunkData, chunkVoxelsSize));

                if (_activeChunksContainer.IsValueExist(chunksPositions[counter]))
                {
                    throw new ArgumentException($"{nameof(ChunksContainer)} hash collision!");
                }

                _activeChunksContainer.AddValue(chunksPositions[counter], chunks[counter]);
                counter++;
            }
        }

        private void ApplyChunksMeshData(
            MeshData[] chunksMeshData,
            IReadOnlyList<IChunk> chunks)
        {
            if (chunksMeshData.Length != chunks.Count)
            {
                throw new ArgumentException();
            }

            for (int i = 0; i < chunks.Count; i++)
            {
                IChunk chunk = chunks[i];

                chunk.ApplyMeshData(chunksMeshData[i]);

                chunk.RootGameObject.transform.localScale
                      = Vector3.one * _basicChunkSettings.Scale;

                chunk.RootGameObject.transform.position
                    = _chunkCoordinatesCalculator.GetGlobalChunkPositionByLocal(chunk.LocalPosition);
            }
        }
    }
}
