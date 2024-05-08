using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using World.Data;

namespace MarchingCubes.MeshGeneration
{
    public struct GPUMeshDataFixJob : IJob
    {
        [ReadOnly]
        public NativeHashSet<int> ExistingMaterialHashes;
        [ReadOnly]
        public NativeArray<Vector3> InputVertices;
        [ReadOnly]
        public NativeArray<IndexAndMaterialHash> InputIndices;
        [ReadOnly]
        public int ChunkID;
        [ReadOnly]
        public int MaxVerticesPerChunk;
        [ReadOnly]
        public int GapValue;
        [ReadOnly]
        public int MaterialsCount;

        [WriteOnly]
        public NativeList<int> OutputIndices;
        [WriteOnly]
        public NativeArray<GPUMeshDataFixJobOutput> Output;

        public NativeArray<Vector3> OutputVertices;
        public NativeList<SubmeshInfo> OutputSubmeshInfos;

        public void Execute()
        {
            unsafe 
            {
                var firstVertexId = MaxVerticesPerChunk * ChunkID;

                int verticesCount = 0;

                Dictionary<int, NativeList<int>> submeshes = new (MaterialsCount);

                for (int i = 0; i < MaxVerticesPerChunk; i++)
                {
                    int inputVertexId = firstVertexId + i;

                    IndexAndMaterialHash indexAndHash = InputIndices[inputVertexId];
                    if (indexAndHash.Index == GapValue 
                    || !ExistingMaterialHashes.Contains(indexAndHash.MaterialHash))
                    {
                        continue;
                    }

                    OutputVertices[verticesCount] = InputVertices[inputVertexId];

                    if (!submeshes.ContainsKey(indexAndHash.MaterialHash))
                    {
                        submeshes.Add(indexAndHash.MaterialHash, new (15, Allocator.Persistent));
                        OutputSubmeshInfos.Add(new SubmeshInfo(indexAndHash.MaterialHash, 0, 0));
                    }

                    submeshes[indexAndHash.MaterialHash].Add(verticesCount);
                    verticesCount++;
                }

                var output = new GPUMeshDataFixJobOutput(
                    CheckIsVerticesCorrect(Mathf.Min(9, verticesCount)),
                    verticesCount
                    );

                Output[ChunkID] = output;

                int previousLength = 0;
                for (int i = 0; i < OutputSubmeshInfos.Length; i++)
                {
                    NativeList<int> submesh = submeshes[OutputSubmeshInfos[i].MaterialHash];
                    SubmeshInfo submeshInfo = OutputSubmeshInfos[i];
                    submeshInfo.IndicesCount = submesh.Length;
                    submeshInfo.IndicesStartIndex = previousLength;
                    previousLength += submesh.Length;
                    OutputSubmeshInfos[i] = submeshInfo;
                    OutputIndices.AddRange(submesh.GetUnsafePtr(), submesh.Length);
                    submesh.Dispose();
                }
            }
        }

        private bool CheckIsVerticesCorrect(int verticesCount)
        {
            if (verticesCount > 2 && verticesCount % 3 == 0)
            {
                int batchesCount = verticesCount / 3;

                for (int batch = 0; batch < batchesCount; batch++)
                {
                    int startId = batch * 3;
                    var vertex = OutputVertices[startId];
                    byte unequalMask = 0;

                    for (int i = startId + 1; i < startId + 3; i++)
                    {
                        var compareVertex = OutputVertices[i];
                        if (vertex.x != compareVertex.x)
                        {
                            unequalMask |= 1;
                        }

                        if (vertex.y != compareVertex.y)
                        {
                            unequalMask |= 1 << 1;
                        }

                        if (vertex.z != compareVertex.z)
                        {
                            unequalMask |= 1 << 2;
                        }
                    }

                    if (!(unequalMask >= 0 && unequalMask <= 2 || unequalMask == 4))
                    {
                        return true;
                    }
                }

                return false;
            }

            return false;
        }
    }
}
