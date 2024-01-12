using System.Threading.Tasks;
using UnityEngine;
using World.Data;

namespace World.Organization
{
    public interface IChunk
    {
        public GameObject RootGameObject { get; }
        /// <summary>
        /// Coordinates of the chunk relative to other chunks.<br/>
        /// For example: <br/>
        /// There is a chunk with local coordinates (4, 2, 5), <br/>
        /// its neighbor on the left will be a chunk with local coordinates (3, 2, 5), 
        /// <br/>and on the right (5, 2, 5)
        /// </summary>
        public Vector3Int LocalPosition { get; }
        public ChunkData ChunkData { get; }

        public void InitializeBasicData(
            MaterialKeyAndUnityMaterialAssociations materialKeyAndUnityMaterial,
            Vector3Int chunkPosition,
            ChunkData chunkData);

        /// <summary>
        /// Saves mesh data locally but don't apply it. <br/>
        /// To apply generated data, invoke method: <br/>
        /// <see cref="ApplyMeshData"/> <br/>
        /// Before invoking this method, make sure the chunk has been initialized
        /// by that method: <br/> 
        /// <see cref="InitializeBasicData"/>
        /// </summary>
        public Task GenerateNewMeshData();

        /// <summary>
        /// Applies locally stored mesh data (Verticies, Triangles, UVs). <br/>
        /// To generate mesh data, invoke method: <br/>
        /// <see cref="GenerateNewMeshData"/>
        /// </summary>
        public void ApplyMeshData();

        public bool IsMeshDataApplying();
    }
}