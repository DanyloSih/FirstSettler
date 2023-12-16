using UnityEngine;

namespace MarchingCubesProject.Tools
{
    public struct ChunkDataVolumeAndMaterial
    {
        private Vector3Int _localChunkPosition;
        private Vector3Int _localChunkDataPoint;
        private float _volume;
        private int _materialHash;

        public ChunkDataVolumeAndMaterial(Vector3Int localChunkPosition, Vector3Int localChunkDataPoint, float volume, int materialHash)
        {
            _localChunkPosition = localChunkPosition;
            _localChunkDataPoint = localChunkDataPoint;
            _volume = volume;
            _materialHash = materialHash;
        }

        public Vector3Int LocalChunkPosition { get => _localChunkPosition; set => _localChunkPosition = value; }
        public Vector3Int LocalChunkDataPoint { get => _localChunkDataPoint; set => _localChunkDataPoint = value; }
        public float Volume { get => _volume; set => _volume = value; }
        public int MaterialHash { get => _materialHash; set => _materialHash = value; }
    }
}
