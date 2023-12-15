using UnityEngine;
using SimpleHeirs;
using System.Collections.Generic;
using System;

namespace MarchingCubesProject
{
    public class ChunksGridGenerator : MonoBehaviour
    {
        [SerializeField] private HeirsProvider<IChunksContainer> _activeChunksContainerHeir;
        [SerializeField] private HeirsProvider<IChunksDataProvider> _chunksDataProviderHeir;
        [SerializeField] private HeirsProvider<IChunk> _chunkPrefab;
        [SerializeField] private Vector3Int _chunksGridSize;

        private List<GameObject> _chunksList = new List<GameObject>();
        private IChunksContainer _activeChunksContainer;
        private GameObject _chunkPrefabGO;
        private IChunksDataProvider _chunksDataProvider;
        private BasicChunkSettings _basicChunkSettings;
        private Vector3Int _minPoint;

        public void OnEnable()
        {
            _activeChunksContainer = _activeChunksContainerHeir.GetValue();
            _chunkPrefabGO = (_chunkPrefab.GetHeirObject() as Component)?.gameObject;
            if (_chunkPrefabGO == null)
            {
                throw new ArgumentException($"{nameof(_chunkPrefab)} should be prefab gameObject!");
            }
            _chunksDataProvider = _chunksDataProviderHeir.GetValue();
            _basicChunkSettings = _chunksDataProvider.BasicChunkSettings;
            _minPoint = Vector3Int.Scale((_chunksGridSize / 2), -Vector3Int.one);

            foreach (var chunk in _chunksList)
            {
                Destroy(chunk.gameObject);
            }

            _activeChunksContainer.ClearAllRecordsAboutChunks();

            FirstInitializationStage();
            SecondInitializationStage();
        }

        private void FirstInitializationStage()
        {
            for (int x = _minPoint.x; x < _chunksGridSize.x; x++)
            {
                for (int y = _minPoint.y; y < _chunksGridSize.y; y++)
                {
                    for (int z = _minPoint.z; z < _chunksGridSize.z; z++)
                    {
                        var instance = Instantiate(_chunkPrefabGO, transform);
                        _chunksList.Add(instance);
                        IChunk chunk = instance.GetComponent(typeof(IChunk)) as IChunk;
                        var chunkData = _chunksDataProvider.GetChunkData(x, y, z);
                        _activeChunksContainer.AddChunk(x, y, z, chunk);
                        chunk.InitializeBasicData(
                            _basicChunkSettings,
                            _chunksDataProvider.MaterialAssociations,
                            new Vector3Int(x, y, z),
                            chunkData);
                    }
                }
            }
        }

        private void SecondInitializationStage()
        {
            for (int x = _minPoint.x; x < _chunksGridSize.x; x++)
            {
                for (int y = _minPoint.y; y < _chunksGridSize.y; y++)
                {
                    for (int z = _minPoint.z; z < _chunksGridSize.z; z++)
                    {
                        IChunk chunk = _activeChunksContainer.GetChunk(x, y, z);

                        chunk.InitializeNeighbors(new ChunkNeighbors(x, y, z, _activeChunksContainer));
                        chunk.UpdateMesh();
                    }
                }
            }
        }
    }
}
