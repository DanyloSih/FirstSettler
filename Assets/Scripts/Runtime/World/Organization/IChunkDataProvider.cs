using System.Threading.Tasks;
using UnityEngine;
using World.Data;

namespace World.Organization
{
    public interface IChunkDataProvider
    {
        public Task<ChunkData> GetChunkData(int x, int y, int z, Vector3Int chunkDataSize);

        public MaterialKeyAndUnityMaterialAssociations MaterialAssociations { get; }
    }
}
