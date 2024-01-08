using UnityEngine;
using Utilities.Math;

namespace World.Organization
{
    public static class ChunkExtensions
    {
        public static long GetUniqueIndex(this IChunk chunk)
        {
            Vector3Int chunkPosition = chunk.LocalPosition;

            var index = PositionHasher.GetPositionHash(
                chunkPosition.x, chunkPosition.y, chunkPosition.z);

            return index;
        }
    }
}