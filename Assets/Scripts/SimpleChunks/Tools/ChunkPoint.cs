using UnityEngine;
using Utilities.Math;

namespace SimpleChunks.Tools
{
    public struct ChunkPoint
    {
        public Vector3Int LocalChunkPosition;
        public Vector3Int LocalVoxelPosition;

        public ChunkPoint(Vector3Int localChunkPosition, Vector3Int localVoxelPosition)
        {
            LocalChunkPosition = localChunkPosition;
            LocalVoxelPosition = localVoxelPosition;
        }

        public Voxel ToVoxel(RectPrismInt chunkVoxelsPrism)
        {
            return ToVoxel(this, chunkVoxelsPrism);
        }

        public static Voxel ToVoxel(ChunkPoint chunkPoint, RectPrismInt chunkVoxelsPrism)
        {
            return ToVoxel(chunkPoint.LocalChunkPosition, chunkPoint.LocalVoxelPosition, chunkVoxelsPrism);
        }

        public static Voxel ToVoxel(Vector3Int chunkPosition, Vector3Int voxelPosition, RectPrismInt chunkVoxelsPrism)
        {
            return new Voxel(
                chunkVoxelsPrism.PointToIndex(voxelPosition),
                PositionLongHasher.GetHashFromPosition(chunkPosition));
        }
    }
}
