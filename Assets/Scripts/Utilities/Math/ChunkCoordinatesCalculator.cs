using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utilities.Math
{
    public struct ChunkCoordinatesCalculator
    {
        private Vector3Int _chunkSizeInCubes;
        private Vector3 _scaledChunkSizeInCubes;
        private Vector3 _halfOfOneVector3;
        private float _scale;

        public ChunkCoordinatesCalculator(Vector3Int chunkSizeInCubes, float scale)
        {
            _chunkSizeInCubes = chunkSizeInCubes;
            _scale = scale;
            _scaledChunkSizeInCubes = new Vector3(
                chunkSizeInCubes.x * scale, chunkSizeInCubes.y * scale, chunkSizeInCubes.z * scale);
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
                localPosition.x * _scaledChunkSizeInCubes.x,
                localPosition.y * _scaledChunkSizeInCubes.y,
                localPosition.z * _scaledChunkSizeInCubes.z
                );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3Int GetLocalChunkPositionByGlobalPoint(Vector3 globalPoint)
        {
            return new Vector3Int(
                Mathf.FloorToInt(globalPoint.x / _scaledChunkSizeInCubes.x),
                Mathf.FloorToInt(globalPoint.y / _scaledChunkSizeInCubes.y),
                Mathf.FloorToInt(globalPoint.z / _scaledChunkSizeInCubes.z)
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
                Mathf.FloorToInt(Mathf.Clamp(localOffset.x, 0, _chunkSizeInCubes.x)),
                Mathf.FloorToInt(Mathf.Clamp(localOffset.y, 0, _chunkSizeInCubes.y)),
                Mathf.FloorToInt(Mathf.Clamp(localOffset.z, 0, _chunkSizeInCubes.z))
                );
        }
    }
}
