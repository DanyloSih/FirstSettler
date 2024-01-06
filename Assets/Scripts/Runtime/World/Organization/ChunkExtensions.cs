using UnityEngine;

namespace World.Organization
{
    public static class ChunkExtensions
    {
        public static long GetUniqueIndexByCoordinates(int x, int y, int z)
        {
            unchecked
            {
                long hash = 54;
                hash = hash * 228 + x.GetHashCode();
                hash = hash * 228 + y.GetHashCode();
                hash = hash * 228 + z.GetHashCode();
                return hash;
            }
        }

        public static long GetUniqueIndex(this IChunk chunk)
        {
            Vector3Int chunkPosition = chunk.LocalPosition;

            var index = GetUniqueIndexByCoordinates(
                chunkPosition.x, chunkPosition.y, chunkPosition.z);

            return index;
        }
    }
}