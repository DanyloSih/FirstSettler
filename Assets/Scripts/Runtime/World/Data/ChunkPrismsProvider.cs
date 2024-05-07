using System;
using Unity.Collections;
using Utilities.Math;
using Zenject;

namespace World.Data
{
    public class ChunkPrismsProvider : IInitializable, IDisposable
    {
        [Inject] private BasicChunkSettings _basicChunkSettings;

        private NativeArray<RectPrismInt> _chunkSizePrisms;

        /// <summary>
        /// First element - chunk cubes prism, <br/>
        /// Second element - chunk voxels prism
        /// </summary>
        public NativeArray<RectPrismInt> ChunkSizePrisms { get => _chunkSizePrisms; }
        public RectPrismInt ChunkCubesPrism => _chunkSizePrisms[0];
        public RectPrismInt ChunkVoxelsPrism => _chunkSizePrisms[1];

        public void Initialize()
        {
            _chunkSizePrisms = new NativeArray<RectPrismInt>(
                2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            _chunkSizePrisms[0] = new RectPrismInt(_basicChunkSettings.Size);
            _chunkSizePrisms[1] = new RectPrismInt(_basicChunkSettings.SizePlusOne);
        }

        public void Dispose()
        {
            _chunkSizePrisms.Dispose();
        }  
    }
}
