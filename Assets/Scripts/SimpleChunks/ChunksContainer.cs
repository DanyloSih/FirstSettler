﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SimpleChunks.DataGeneration;
using Unity.Collections;
using Utilities.Jobs;
using Utilities.Math;
using Zenject;

namespace SimpleChunks
{
    public class ChunksContainer : IPositionalHashMap<IChunk>, IInitializable, IDisposable
    {
        private const int INITIAL_CAPACITY = 512;

        private Dictionary<long, IChunk> _chunks = new Dictionary<long, IChunk>(INITIAL_CAPACITY);
        private NativeParallelHashMapManager<long, UnsafeNativeArray<VoxelData>> _nativeParallelHashMapManager;

        public int MaxCoordinateValue => PositionIntHasher.Y_MAX;
        public int MinCoordinate => -PositionIntHasher.Y_MAX;

        public void Initialize()
        {
            _nativeParallelHashMapManager = new(INITIAL_CAPACITY);
        }

        public void Dispose()
        {
            _nativeParallelHashMapManager.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetNativeParallelHashMap(ReadParallelHashMapDelegate<long, UnsafeNativeArray<VoxelData>> readFunction)
        {
            _nativeParallelHashMapManager.GetReadOnly(readFunction);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddValue(int x, int y, int z, IChunk chunk)
        {
            long hash = PositionLongHasher.GetHashFromPosition(x, y, z);
            _nativeParallelHashMapManager.Add(hash, new UnsafeNativeArray<VoxelData>(chunk.ChunkData.RawData, Allocator.Persistent));
            _chunks.Add(hash, chunk);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveValue(int x, int y, int z)
        {
            long hash = PositionLongHasher.GetHashFromPosition(x, y, z);
            _nativeParallelHashMapManager.Remove(hash);
            _chunks.Remove(hash);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(int x, int y, int z, out IChunk result)
        {
            return _chunks.TryGetValue(PositionLongHasher.GetHashFromPosition(x, y, z), out result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(long positionHash, out IChunk result)
        {
            return _chunks.TryGetValue(positionHash, out result);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValueExist(int x, int y, int z)
        {
            return _chunks.ContainsKey(PositionLongHasher.GetHashFromPosition(x, y, z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearAllRecords()
        {
            _chunks.Clear();
        }    
    }
}
