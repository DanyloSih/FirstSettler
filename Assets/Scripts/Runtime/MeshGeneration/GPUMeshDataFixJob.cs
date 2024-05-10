using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using World.Data;
using Debug = UnityEngine.Debug;

namespace MeshGeneration
{
    public struct GPUMeshDataFixJob : IJob
    {
        [ReadOnly, NativeDisableContainerSafetyRestriction]
        public NativeHashSet<int> ExistingMaterialHashes;
        [ReadOnly, NativeDisableContainerSafetyRestriction]
        public NativeArray<Vector3> InputVertices;
        [ReadOnly, NativeDisableContainerSafetyRestriction]
        public NativeArray<VertexInfo> InputVerticesInfo;
        [ReadOnly]
        public int ChunkID;
        [ReadOnly]
        public int MaxVerticesPerChunk;
        [ReadOnly]
        public int MaterialsCount;

        [WriteOnly, NativeDisableContainerSafetyRestriction]
        public NativeList<int> OutputIndices;
        [WriteOnly, NativeDisableContainerSafetyRestriction]
        public NativeArray<GPUMeshDataFixJobOutput> Output;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<Vector3> OutputVertices;
        [NativeDisableContainerSafetyRestriction]
        public NativeList<SubmeshInfo> OutputSubmeshInfos;

        public void Execute()
        {
            unsafe 
            {
                var firstVertexId = MaxVerticesPerChunk * ChunkID;

                int verticesCount = 0;
                //Stopwatch stopwatch = Stopwatch.StartNew();
                //Output[0] = new GPUMeshDataFixJobOutput(false, verticesCount);

                Dictionary<int, NativeList<int>> submeshes = new (MaterialsCount);

                for (int i = 0; i < MaxVerticesPerChunk; i += 3)
                {
                    int inputVertexId = firstVertexId + i;

                    VertexInfo vertexInfo = InputVerticesInfo[inputVertexId];

                    if (!vertexInfo.IsCorrect)
                    {
                        continue;
                    }

                    OutputVertices[verticesCount] = InputVertices[inputVertexId];
                    OutputVertices[verticesCount + 1] = InputVertices[inputVertexId + 1];
                    OutputVertices[verticesCount + 2] = InputVertices[inputVertexId + 2];

                    if (!submeshes.ContainsKey(vertexInfo.MaterialHash))
                    {
                        submeshes.Add(vertexInfo.MaterialHash, new (15, Allocator.Persistent));
                        OutputSubmeshInfos.Add(new SubmeshInfo(vertexInfo.MaterialHash, 0, 0));
                    }

                    submeshes[vertexInfo.MaterialHash].Add(verticesCount);
                    verticesCount++;
                    submeshes[vertexInfo.MaterialHash].Add(verticesCount);
                    verticesCount++;
                    submeshes[vertexInfo.MaterialHash].Add(verticesCount);
                    verticesCount++;
                }

                //Debug.Log($"First part done in {stopwatch.ElapsedMilliseconds}");
                //stopwatch.Restart();
                //Output[0] = new GPUMeshDataFixJobOutput(false, verticesCount);

                var output = new GPUMeshDataFixJobOutput(
                    CheckIsVerticesCorrect(Mathf.Min(9, verticesCount)),
                    verticesCount
                    );

                Output[0] = output;

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

                //Debug.Log($"Second part done in {stopwatch.ElapsedMilliseconds}");
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
