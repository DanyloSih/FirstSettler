using System.Threading.Tasks;
using UnityEngine;
using World.Data;

namespace World.Organization
{
    public interface IChunkDataProvider
    {
        public Task FillChunkData(ChunkData chunkData, int chunkLocalX, int chunkLocalY, int chunkLocalZ);

        public MaterialKeyAndUnityMaterialAssociations MaterialAssociations { get; }
    }
}
