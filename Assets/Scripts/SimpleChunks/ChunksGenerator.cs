using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SimpleChunks.DataGeneration;
using SimpleChunks.MeshGeneration;
using Unity.Collections;
using UnityEngine;
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

        [Inject]
        public void Construct(DiContainer diContainer,
            BasicChunkSettings basicChunkSettings,
            ChunkCoordinatesCalculator chunkCoordinatesCalculator,
            Transform chunksRoot,
            MeshGenerator meshGenerator,
            IChunksContainer chunksContainer,
            IChunkDataProvider chunkDataProvider,
            IChunk chunkPrefab)
        {
            _diContainer = diContainer;
            _basicChunkSettings = basicChunkSettings;
            _activeChunksContainer = chunksContainer;
            _chunksDataProvider = chunkDataProvider;
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

            Debug.Log("Chunks generated.");
        }

        private async Task GenerateBatch(
            NativeArray<Vector3Int> chunkPositionsArray, 
            CancellationToken? cancellationToken = null)
        {
            IReadOnlyList<IChunk> chunksGameObjects
                = CreateChunksGameObject(chunkPositionsArray);

            List<ThreedimensionalNativeArray<VoxelData>> chunksData
                = await _chunksDataProvider.GenerateChunksRawData(
                    chunkPositionsArray, cancellationToken);

            InitializeChunks(chunksGameObjects, chunkPositionsArray, chunksData);

            MeshData[] chunksMeshData = await _meshGenerator
                .GenerateMeshDataForChunks(chunksData, cancellationToken)
                .OnException(ex => Debug.LogException(ex));

            ApplyChunksMeshData(chunksMeshData, chunksGameObjects);
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

        private void InitializeChunks(
            IReadOnlyList<IChunk> chunks, 
            NativeArray<Vector3Int> chunksPositions, 
            List<ThreedimensionalNativeArray<VoxelData>> chunksData)
        {
            for (int i = 0; i < chunks.Count; i++)
            {
                chunks[i].InitializeBasicData(chunksPositions[i], chunksData[i]);
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
                chunksMeshData[i].Dispose();

                chunk.RootGameObject.transform.localScale
                      = Vector3.one * _basicChunkSettings.Scale;

                chunk.RootGameObject.transform.position
                    = _chunkCoordinatesCalculator.GetGlobalChunkPositionByLocal(chunk.LocalPosition);
            }
        }
    }
}
