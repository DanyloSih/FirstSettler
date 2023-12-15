using System.Collections.Generic;
using System.Linq;
using ProceduralNoiseProject;
using UnityEngine;

namespace MarchingCubesProject
{
    public class RealtimeChunksDataProvider : MonoBehaviour, IChunksDataProvider
    {
        [SerializeField] private BasicChunkSettings _basicChunkSettings;
        [SerializeField] private MaterialKeyAndUnityMaterialAssociations _materialAssociations;
        [SerializeField] private float _maxHeight = 256;
        [SerializeField] private float _minHeight;
        [SerializeField] private float _smooth;
        [SerializeField] private int _seed = 0;

        private FractalNoise _fractal;
        private int _width;
        private int _height;
        private int _depth;

        public BasicChunkSettings BasicChunkSettings { get => _basicChunkSettings; }
        public MaterialKeyAndUnityMaterialAssociations MaterialAssociations { get => _materialAssociations; }

        private void UpdateNoise()
        {
            INoise perlin = new PerlinNoise(_seed, 1.0f);
            _fractal = new FractalNoise(perlin, 3, 1.0f);
        }

        private ChunkData UpdateVoxelArray(float xOffset, float yOffset, float zOffset)
        {
            IEnumerable<int> hashes = _materialAssociations.GetMaterialKeyHashes();
            int keysCount = hashes.Count();

            var chunkData = new ChunkData(_width, _height, _depth);
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    for (int z = 0; z < _depth; z++)
                    {
                        float u = x / (_width - 1.0f);
                        float v = y / (_height - 1.0f);
                        float w = z / (_depth - 1.0f);
                        float globalX = x + xOffset * _width;
                        float globalY = y + yOffset * _height;
                        float globalZ = z + zOffset * _depth;

                        var heightThreshold = _fractal.Sample2D(u + xOffset, w + zOffset) * _maxHeight;
                        float currentVolume = globalY < heightThreshold ? 1 : 0;
                        currentVolume = globalY <= _minHeight ? 1 : currentVolume;
                        chunkData.SetVolume(x, y, z, currentVolume);
                        var hash = hashes.ElementAt(Random.Range(0, keysCount));
                        chunkData.SetMaterialHash(x, y, z, hash);
                    }
                }
            }

            return chunkData;
        }

        public ChunkData GetChunkData(int x, int y, int z)
        {
            _width = _basicChunkSettings.Width;
            _height = _basicChunkSettings.Height;
            _depth = _basicChunkSettings.Depth;
            UpdateNoise();
            return UpdateVoxelArray(x, y, z);
        }
    }
}
