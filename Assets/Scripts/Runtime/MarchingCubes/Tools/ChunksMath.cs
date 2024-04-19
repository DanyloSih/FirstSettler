using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Utilities.Math;
using World.Organization;

namespace MarchingCubesProject.Tools
{
    public static class ChunksMath
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeHashMap<int, IntPtr> GetChunksDataPointersInsideArea(
            ShapeIntArea<RectPrismInt> area, Vector3Int chunksSize, IChunksContainer chunksContainer)
        {
            Vector3Int affectedAreaSize = area.Shape.Size;
            int maxXChunks = affectedAreaSize.x / chunksSize.x + 1;
            int maxYChunks = affectedAreaSize.y / chunksSize.y + 1;
            int maxZChunks = affectedAreaSize.z / chunksSize.z + 1;
            int maxAffectedChunksCount = maxXChunks * maxYChunks * maxZChunks;

            Vector3Int min = area.Anchor;
            Vector3Int max = area.Anchor + area.Shape.Size;

            NativeHashMap<int, IntPtr> pointers
                = new NativeHashMap<int, IntPtr>(maxAffectedChunksCount, Allocator.Persistent);

            for (int y = Mathf.FloorToInt((float)min.y / chunksSize.y) * chunksSize.y; y < max.y; y += chunksSize.y)
            {
                for (int x = Mathf.FloorToInt((float)min.x / chunksSize.x) * chunksSize.x; x < max.x; x += chunksSize.x)
                {
                    for (int z = Mathf.FloorToInt((float)min.z / chunksSize.z) * chunksSize.z; z < max.z; z += chunksSize.z)
                    {
                        int localChunkX = x / chunksSize.x;
                        int localChunkY = y / chunksSize.y;
                        int localChunkZ = z / chunksSize.z;
                        int positionHash = PositionHasher.GetPositionHash(localChunkX, localChunkY, localChunkZ);
                        IChunk chunk = chunksContainer.GetChunk(positionHash);

                        if (chunk == null)
                        {
                            continue;
                        }

                        unsafe
                        {
                            pointers.Add(positionHash, new IntPtr(chunk.ChunkData.VoxelsData.RawData.GetUnsafePtr()));
                        };
                    }
                }
            }

            return pointers;
        }
    }
}
