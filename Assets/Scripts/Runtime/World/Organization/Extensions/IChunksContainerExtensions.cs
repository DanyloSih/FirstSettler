using UnityEngine;

namespace World.Organization.Extensions
{
    public static class IChunksContainerExtensions
    {
        /// <summary>
        /// <inheritdoc cref="IChunksContainer.GetChunk(int, int, int)"/>
        /// </summary>
        public static IChunk GetChunk(this IChunksContainer chunksContainer, Vector3Int chunkPosition)
        {
            return chunksContainer.GetChunk(chunkPosition.x, chunkPosition.y, chunkPosition.z);
        }

        /// <summary>
        /// <inheritdoc cref="IChunksContainer.AddChunk(int, int, int, IChunk)"/>
        /// </summary>
        public static void AddChunk(this IChunksContainer chunksContainer, Vector3Int chunkPosition, IChunk chunk)
        {
            chunksContainer.AddChunk(chunkPosition.x, chunkPosition.y, chunkPosition.z, chunk);
        }

        /// <summary>
        /// <inheritdoc cref="IChunksContainer.RemoveChunk(int, int, int)"/>
        /// </summary>
        public static void RemoveChunk(this IChunksContainer chunksContainer, Vector3Int chunkPosition)
        {
            chunksContainer.RemoveChunk(chunkPosition.x, chunkPosition.y, chunkPosition.z);
        }

        /// <summary>
        /// <inheritdoc cref="IChunksContainer.IsChunkExist(int, int, int)"/>
        /// </summary>
        public static bool IsChunkExist(this IChunksContainer chunksContainer, Vector3Int chunkPosition)
        {
            return chunksContainer.IsChunkExist(chunkPosition.x, chunkPosition.y, chunkPosition.z);
        }
    }
}
