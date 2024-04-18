using UnityEngine;
using Utilities.Math.Extensions;

namespace World.Organization
{
    public class ChunksGridGenerator : ChunksGeneratorBase
    {
        [SerializeField] private Vector3Int _chunksGridSize;

        private IMatrixWalker _matrixWalker;
        private Vector3Int _minPoint;
        private Vector3Int _loadingGridSize;

        protected async override void InitializeChunks()
        {
            _loadingGridSize = _chunksGridSize.GetElementwiseFloorDividedVector(ChunksLoadingVolumePerCall);
            _chunksGridSize = Vector3Int.Scale(_loadingGridSize, ChunksLoadingVolumePerCall);
            _minPoint = _chunksGridSize / 2;
            _matrixWalker = new SpiralMatrixWalker();

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            foreach (var batchPos in _matrixWalker.WalkMatrix(_loadingGridSize))
            {
                Vector3Int loadingChunksStartPos = Vector3Int.Scale(batchPos, ChunksLoadingVolumePerCall) - _minPoint;
                Vector3Int loadingChunksEndPos = loadingChunksStartPos + ChunksLoadingVolumePerCall;
                await GenerateChunksBatch(loadingChunksStartPos, loadingChunksEndPos);
            }
            stopwatch.Stop();

            Debug.Log($"Chunks loading ended in: {stopwatch.Elapsed.TotalSeconds} seconds");
        }
    }
}
