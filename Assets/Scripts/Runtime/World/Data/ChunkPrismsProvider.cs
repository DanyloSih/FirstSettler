using System;
using Unity.Collections;
using Utilities.Math;
using Zenject;

namespace World.Data
{
    public struct ChunkPrismsProvider : IInitializable, IDisposable
    {
        private NativeArray<RectPrismInt> _chunkSizePrisms;
        private RectPrismInt _chunkVoxelsPrism;
        private RectPrismInt _chunkCubesPrism;

        public ChunkPrismsProvider(
            BasicChunkSettings basicChunkSettings)
        {
            _chunkSizePrisms = new NativeArray<RectPrismInt>();
            _chunkCubesPrism = new RectPrismInt(basicChunkSettings.Size);
            _chunkVoxelsPrism = new RectPrismInt(basicChunkSettings.SizePlusOne);
        }

        /// <summary>
        /// First element - chunk cubes prism, <br/>
        /// Second element - chunk voxels prism
        /// </summary>
        public NativeArray<RectPrismInt> PrismsArray 
        { 
            get
            {
                if (!_chunkSizePrisms.IsCreated)
                {
                    Initialize(); 
                }
                
                return _chunkSizePrisms;
            }
        }
        public RectPrismInt CubesPrism => _chunkCubesPrism;
        public RectPrismInt VoxelsPrism => _chunkVoxelsPrism;

        public void Initialize()
        {
            _chunkSizePrisms = new NativeArray<RectPrismInt>(
                2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            _chunkSizePrisms[0] = _chunkCubesPrism;
            _chunkSizePrisms[1] = _chunkVoxelsPrism;
        }

        public void Dispose()
        {
            _chunkSizePrisms.Dispose();
        }  
    }
}
