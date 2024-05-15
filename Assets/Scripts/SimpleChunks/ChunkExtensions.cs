using UnityEngine;
using Utilities.Math;

namespace SimpleChunks
{
    public static class ChunkExtensions
    {
        public static int GetUniqueIndex(this IChunk chunk)
        {
            Vector3Int chunkPosition = chunk.LocalPosition;

            int index = PositionIntHasher.GetHashFromPosition(chunkPosition);

            return index;
        }
    }
}