using System.Threading.Tasks;
using UnityEngine;
using World.Data;

namespace World.Organization
{
    public interface IChunkDataProvider
    {
        public Task FillChunkData(ChunkData chunkData, Vector3Int chunkGlobalPosition);

        public MaterialKeyAndUnityMaterialAssociations MaterialAssociations { get; }
    }
}
