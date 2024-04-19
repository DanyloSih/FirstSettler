using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.Math;
using World.Data;

namespace World.Organization
{
    public interface IChunkDataProvider
    {
        public Task<List<ThreedimensionalNativeArray<VoxelData>>> GenerateChunksRawData(
            RectPrismAreaInt loadingArea, Vector3Int chunkOffset, Vector3Int chunkDataSize);

        public MaterialKeyAndUnityMaterialAssociations MaterialAssociations { get; }
    }
}
