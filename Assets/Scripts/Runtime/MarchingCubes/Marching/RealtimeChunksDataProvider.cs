﻿using System;
using System.Collections.Generic;
using System.Linq;
using ProceduralNoiseProject;
using UnityEngine;
using World.Data;
using World.Organization;

namespace MarchingCubesProject
{
    public class RealtimeChunksDataProvider : MonoBehaviour, IChunksDataProvider
    {
        [SerializeField] private float _maxHeight = 256;
        [SerializeField] private float _heightOffset;
        [SerializeField] private int _octaves;
        [SerializeField] private float _frequency;
        [SerializeField] private int _seed = 0;
        [SerializeField] private MaterialKeyAndUnityMaterialAssociations _materialAssociations;
        [SerializeField] private List<HeightAndMaterialKeyAssociation> _heightAndMaterialKeyAssociations;

        private FractalNoise _fractal;
        private int _width;
        private int _height;
        private int _depth;

        public MaterialKeyAndUnityMaterialAssociations MaterialAssociations
            => _materialAssociations;

        private void UpdateNoise()
        {
            INoise perlin = new PerlinNoise(_seed, _frequency);
            _fractal = new FractalNoise(perlin, _octaves, _frequency);
        }

        private ChunkData UpdateVoxelArray(float xOffset, float yOffset, float zOffset)
        {
            var chunkData = new ChunkData(_width, _height, _depth);
            for (int y = 0; y < _height; y++)
            {
                float v = y / (_height - 1.0f);
                float globalY = y + yOffset * _height;
                var hash = _heightAndMaterialKeyAssociations
                    .FindLast(association => globalY < association.Height).MaterialKey.GetHashCode();

                for (int x = 0; x < _width; x++)
                {
                    float u = x / (_width - 1.0f);
                    float globalX = x + xOffset * _width;

                    for (int z = 0; z < _depth; z++)
                    {
                        float w = z / (_depth - 1.0f);
                        float globalZ = z + zOffset * _depth;

                        var heightThreshold = (1 + _fractal.Sample2D(u + xOffset, w + zOffset)) / 2 * _maxHeight + _heightOffset;
                        float currentVolume = globalY < heightThreshold ? 1 : 0;
                        chunkData.SetVolume(x, y, z, currentVolume);
                        
                        chunkData.SetMaterialHash(x, y, z, hash);
                    }
                }
            }

            return chunkData;
        }

        public ChunkData GetChunkData(int x, int y, int z, Vector3Int chunkDataSize)
        {
            _width = chunkDataSize.x;
            _height = chunkDataSize.y;
            _depth = chunkDataSize.z;
            UpdateNoise();
            return UpdateVoxelArray(x, y, z);
        }
    }
}
