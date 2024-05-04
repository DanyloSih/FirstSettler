using System.Threading.Tasks;
using UnityEngine;
using Utilities.Math;
using Utilities.Math.Extensions;

namespace World.Organization
{
    public class ChunksGridGenerator : ChunksGenerationBehaviour
    {
        [SerializeField] private Vector3Int _chunksGridSize;

        private IMatrixWalker _matrixWalker;
        private Vector3Int _minPoint;
        private Vector3Int _loadingGridSize;
        private Task _currentTask;

        public override async Task AwaitGenerationProcess()
        {
            if(_currentTask == null || _currentTask.IsCompleted) 
            {
                return;
            }

            await _currentTask;
        }

        protected async override void OnGenerationProcessStart()
        {
            //_loadingGridSize = _chunksGridSize.GetElementwiseFloorDividedVector(ChunksLoadingVolumePerCall);
            //_chunksGridSize = Vector3Int.Scale(_loadingGridSize, ChunksLoadingVolumePerCall);
            //_minPoint = _chunksGridSize / 2;
            //_matrixWalker = new SpiralMatrixWalker();

            //System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            //_currentTask = GenerateChunksMatrix();
            //await _currentTask;
            //stopwatch.Stop();

            //Debug.Log($"Chunks loading ended in: {stopwatch.Elapsed.TotalSeconds} seconds");
        }

        private async Task GenerateChunksMatrix()
        {
            //foreach (var batchPos in _matrixWalker.WalkMatrix(_loadingGridSize))
            //{
            //    Vector3Int loadingChunksStartPos = Vector3Int.Scale(batchPos, ChunksLoadingVolumePerCall) - _minPoint;
            //    Vector3Int loadingChunksEndPos = loadingChunksStartPos + ChunksLoadingVolumePerCall;
            //    await GenerateChunksBatch(loadingChunksStartPos, loadingChunksEndPos);
            //}
        }

        protected override void OnGenerationProcessStop()
        {
            
        }
    }
}
