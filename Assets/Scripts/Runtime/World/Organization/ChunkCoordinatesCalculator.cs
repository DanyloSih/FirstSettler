using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace World.Organization
{
    public class ChunkCoordinatesCalculator
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
        public Vector3Int GetLocalChunkPositionByGlobalPoint(Vector3 globalPoint)
        {
            return new Vector3Int(
                Mathf.FloorToInt(globalPoint.x / _scaledChunkSize.x),
                Mathf.FloorToInt(globalPoint.y / _scaledChunkSize.y),
                Mathf.FloorToInt(globalPoint.z / _scaledChunkSize.z)
                );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 GetGlobalChunkPositionByLocal(Vector3Int localPosition, bool useScaledChunkSize = true)
        {
            if (useScaledChunkSize)
            {
                return new Vector3(
                    localPosition.x * _scaledChunkSize.x,
                    localPosition.y * _scaledChunkSize.y,
                    localPosition.z * _scaledChunkSize.z
                    );
            }
            else
            {
                return new Vector3(
                   localPosition.x * _chunkSize.x,
                   localPosition.y * _chunkSize.y,
                   localPosition.z * _chunkSize.z
                   );
            }
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3Int GetGlobalChunkDataPointByGlobalPoint(Vector3 globalPoint)
        {
            Vector3Int localChunkPosition = GetLocalChunkPositionByGlobalPoint(globalPoint);
            Vector3 globalChunkPosition = GetGlobalChunkPositionByLocal(localChunkPosition, false);
            Vector3Int localChunkDataPoint = GetLocalChunkDataPointByGlobalPoint(globalPoint);
            Vector3 globalChunkDataPoint = localChunkDataPoint + globalChunkPosition ;

            return new Vector3Int(
                (int)globalChunkDataPoint.x, 
                (int)globalChunkDataPoint.y, 
                (int)globalChunkDataPoint.z);
        }
    }
}
