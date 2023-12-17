using UnityEngine;

namespace MarchingCubesProject.Tools
{
    public struct ChunkDataPoint
    {
        private Vector3 _globalChunkDataPoint;
        private float _volume;
        private int _materialHash;

        public ChunkDataPoint(Vector3 globalChunkDataPoint, float volume, int materialHash)
        {
            _globalChunkDataPoint = globalChunkDataPoint;
            _volume = volume;
            _materialHash = materialHash;
        }

        public Vector3 GlobalChunkDataPoint { get => _globalChunkDataPoint; set => _globalChunkDataPoint = value; }
        public float Volume { get => _volume; set => _volume = value; }
        public int MaterialHash { get => _materialHash; set => _materialHash = value; }
    }
}
