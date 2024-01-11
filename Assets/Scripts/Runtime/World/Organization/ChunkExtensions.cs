﻿using UnityEngine;
using Utilities.Math;

namespace World.Organization
{
    public static class ChunkExtensions
    {
        public static int GetUniqueIndex(this IChunk chunk)
        {
            Vector3Int chunkPosition = chunk.LocalPosition;

            int index = PositionHasher.GetPositionHash(chunkPosition);

            return index;
        }
    }
}