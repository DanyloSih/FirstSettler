using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using Utilities.Math;
using World.Data;

namespace MeshGeneration
{

    public abstract class MeshGenerator : MonoBehaviour
    {
        public abstract Task<MeshData[]> GenerateMeshDataForChunks(
            NativeArray<Vector3Int> positions,
            List<ThreedimensionalNativeArray<VoxelData>> chunksData, 
            CancellationToken? cancellationToken = null);
    }
}
