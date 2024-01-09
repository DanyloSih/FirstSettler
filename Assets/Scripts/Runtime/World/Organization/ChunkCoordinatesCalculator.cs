using System.Runtime.CompilerServices;
using UnityEngine;

namespace World.Organization
{
    public struct ChunkCoordinatesCalculator
    {
        private Vector3Int _chunkSize;
        private Vector3 _scaledChunkSize;
        private Vector3 _halfOfOneVector3;
        private float _scale;

        public ChunkCoordinatesCalculator(Vector3Int chunkSize, float scale)
        {
            _chunkSize = chunkSize;
            _scale = scale;
            _scaledChunkSize = new Vector3(
                chunkSize.x * scale, chunkSize.y * scale, chunkSize.z * scale);
            _halfOfOneVector3 = Vector3.one / 2f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 GetGlobalChunkDataPointByGlobalPoint(Vector3 globalPoint)
        {
            Vector3Int localChunkPosition = GetLocalChunkPositionByGlobalPoint(globalPoint);
            Vector3 globalChunkPosition = GetGlobalChunkPositionByLocal(localChunkPosition);
            Vector3 localChunkDataPoint = GetLocalChunkDataPointByGlobalPoint(globalPoint);
            Vector3 globalChunkDataPoint = localChunkDataPoint * _scale + globalChunkPosition;

            return globalChunkDataPoint;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 GetGlobalChunkDataPointByLocalChunkAndPoint(
            Vector3Int localChunkPosition, Vector3Int localChunkDataPoint)
        {
            Vector3 globalChunkPosition = GetGlobalChunkPositionByLocal(localChunkPosition);
            Vector3 globalChunkDataPoint = ((Vector3)localChunkDataPoint) * _scale + globalChunkPosition;
            return globalChunkDataPoint;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 GetGlobalChunkPositionByLocal(Vector3Int localPosition)
        {
            return new Vector3(
                localPosition.x * _scaledChunkSize.x,
                localPosition.y * _scaledChunkSize.y,
                localPosition.z * _scaledChunkSize.z
                );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3Int GetLocalChunkPositionByGlobalPoint(Vector3 globalPoint)
        {
            return new Vector3Int(
                Mathf.FloorToInt(globalPoint.x / _scaledChunkSize.x),
                Mathf.FloorToInt(globalPoint.y / _scaledChunkSize.y),
                Mathf.FloorToInt(globalPoint.z / _scaledChunkSize.z)
                );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3Int GetLocalChunkDataPointByGlobalPoint(Vector3 globalPoint)
        {
            Vector3Int localChunkPosition = GetLocalChunkPositionByGlobalPoint(globalPoint);
            Vector3 globalChunkPosition = GetGlobalChunkPositionByLocal(localChunkPosition);
            Vector3 localOffset = globalPoint;
            localOffset -= globalChunkPosition;
            localOffset += _halfOfOneVector3 * _scale;
            localOffset /= _scale;
            return new Vector3Int(
                Mathf.FloorToInt(Mathf.Clamp(localOffset.x, 0, _chunkSize.x)),
                Mathf.FloorToInt(Mathf.Clamp(localOffset.y, 0, _chunkSize.y)),
                Mathf.FloorToInt(Mathf.Clamp(localOffset.z, 0, _chunkSize.z))
                );
        }
    }
}
