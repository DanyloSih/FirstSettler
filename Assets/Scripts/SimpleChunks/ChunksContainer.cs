using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Utilities.Math;

namespace SimpleChunks
{
    public class ChunksContainer : MonoBehaviour, IChunksContainer
    {
        /// <summary>
        /// Limit per coordinate: from -1 321 122 to 1 321 122<br/>
        /// For example, if you set x to 2 321 129 or -1 521 122, this may cause problems.
        /// </summary>
        public const int MAX_CHUNK_ID_IN_ONE_DIRECTION = 1321122;

        private static readonly int s_maxChunkID = MAX_CHUNK_ID_IN_ONE_DIRECTION;
        private static readonly int s_maxChunkIDSquared = s_maxChunkID * s_maxChunkID;

        private Dictionary<int, IChunk> _chunks = new Dictionary<int, IChunk>();

        public int MaxCoordinateValue { get => MAX_CHUNK_ID_IN_ONE_DIRECTION; }
        public int MinCoordinate { get => -MAX_CHUNK_ID_IN_ONE_DIRECTION; }

        /// <summary>
        /// <inheritdoc cref="IChunksContainer.AddChunk(int, int, int, IChunk)"/><br/>
        /// <inheritdoc cref="MAX_CHUNK_ID_IN_ONE_DIRECTION"/><br/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddChunk(int x, int y, int z, IChunk chunk)
        {
            _chunks.Add(PositionHasher.GetPositionHash(x, y, z), chunk);
        }

        /// <summary>
        /// <inheritdoc cref="IChunksContainer.RemoveChunk(int, int, int)"/><br/>
        /// <inheritdoc cref="MAX_CHUNK_ID_IN_ONE_DIRECTION"/><br/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveChunk(int x, int y, int z)
        {
            _chunks.Remove(PositionHasher.GetPositionHash(x, y, z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IChunk GetChunk(int x, int y, int z)
        {
            if (_chunks.TryGetValue(PositionHasher.GetPositionHash(x, y, z), out var chunk))
            {
                return chunk;
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IChunk GetChunk(int positionHash)
        {
            if (_chunks.TryGetValue(positionHash, out var result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// <inheritdoc cref="IChunksContainer.IsChunkExist(int, int, int)"/><br/>
        /// <inheritdoc cref="MAX_CHUNK_ID_IN_ONE_DIRECTION"/><br/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsChunkExist(int x, int y, int z)
        {
            return _chunks.ContainsKey(PositionHasher.GetPositionHash(x, y, z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearAllRecordsAboutChunks()
        {
            _chunks.Clear();
        }
    }
}
