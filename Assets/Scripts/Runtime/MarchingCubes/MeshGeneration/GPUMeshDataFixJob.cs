using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using World.Data;
using Debug = UnityEngine.Debug;

namespace MarchingCubes.MeshGeneration
{
    public struct GPUMeshDataFixJob : IJob
    {
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
        public int VerticesCount;
        [WriteOnly]
        public bool IsPhysicallyCorrect;

        public NativeArray<Vector3> OutputVertices;
        public NativeList<SubmeshInfo> OutputSubmeshInfos;

        public void Execute()
        {
            //Debug.Log($"Job for chunk with id: \"{ChunkID}\" START!");
            //Stopwatch stopwatch = Stopwatch.StartNew();
            unsafe 
            {
                var firstVertexId = MaxVerticesPerChunk * ChunkID;

                int verticesCount = 0;

                UnsafeHashMap<int, UnsafeList<int>> submeshes = new (MaterialsCount, Allocator.Persistent);

                for (int i = 0; i < MaxVerticesPerChunk; i += 3)
                {
                    int inputVertexId = firstVertexId + i;

                    IndexAndMaterialHash indexAndHash = InputIndices[inputVertexId];
                    if (indexAndHash.Index == GapValue)
                    {
                        continue;
                    }

                    OutputVertices[verticesCount] = InputVertices[inputVertexId];
                    OutputVertices[verticesCount + 1] = InputVertices[inputVertexId + 1];
                    OutputVertices[verticesCount + 2] = InputVertices[inputVertexId + 2];

                    if (!submeshes.ContainsKey(indexAndHash.MaterialHash))
                    {
                        submeshes.Add(indexAndHash.MaterialHash, new (15, Allocator.Persistent, NativeArrayOptions.UninitializedMemory));
                        OutputSubmeshInfos.Add(new SubmeshInfo(indexAndHash.MaterialHash, i, 0));
                    }

                    UnsafeList<int> submesh = submeshes[indexAndHash.MaterialHash];
                    
                    submesh.Add(indexAndHash.Index);
                    submesh.Add(InputIndices[inputVertexId + 1].Index);
                    submesh.Add(InputIndices[inputVertexId + 2].Index);
                    verticesCount += 3;
                }

                VerticesCount = verticesCount;
                IsPhysicallyCorrect = CheckIsVerticesCorrect(9);

                for (int i = 0; i < OutputSubmeshInfos.Length; i++)
                {
                    UnsafeList<int> submesh = submeshes[OutputSubmeshInfos[i].MaterialHash];
                    SubmeshInfo submeshInfo = OutputSubmeshInfos[i];
                    submeshInfo.IndicesCount = submesh.Length;
                    OutputSubmeshInfos[i] = submeshInfo;
                    OutputIndices.AddRange(submesh.Ptr, submesh.Length);
                    submesh.Dispose();
                }
                submeshes.Dispose();
            }

            //Debug.Log($"Job for chunk with id: \"{ChunkID}\" DONE in \"{stopwatch.ElapsedMilliseconds}\" milliseconds!");
        }

        private bool CheckIsVerticesCorrect(int maxVerticesCount)
        {
            var verticesCount = Mathf.Min(maxVerticesCount, OutputVertices.Length);
            if (verticesCount > 2 && verticesCount % 3 == 0)
            {
                int batchesCount = verticesCount / 3;

                for (int batch = 0; batch < batchesCount; batch++)
                {
                    int startId = batch * 3;
                    var vertex = OutputVertices[startId];
                    byte unequalMask = 0;

                    for (int i = startId + 1; i < verticesCount; i++)
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

                    if (unequalMask >= 0 && unequalMask <= 2 || unequalMask == 4)
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
