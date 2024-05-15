using UnityEngine;
using Utilities.Math;

namespace SimpleChunks
{
    public static class ChunkExtensions
    {
        public static long GetUniqueIndex(this IChunk chunk)
        {
            Vector3Int chunkPosition = chunk.LocalPosition;

            long index = PositionLongHasher.GetHashFromPosition(chunkPosition);

            return index;
        }
    }
}