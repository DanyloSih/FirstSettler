using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using Utilities.Math;
using Utilities.Math.Extensions;
using Zenject;

namespace World.Organization
{
    public class DynamicChunksGridGenerator : ChunksGeneratorBase
    {
        [Inject] private ChunkPositionProvider _generationChunkPositionProvider;

        [SerializeField] private int _viewDistance = 16;
        [BoxGroup("Chunks disposing params")]
        [SerializeField] private int _disposeBatchLength = 4;
        [BoxGroup("Chunks disposing params")]
        [Tooltip("Delay in milliseconds")]
        [SerializeField] private int _disposeBatchDelay = 1;

        private ShapeIntArea<SphereInt>? _previousViewShape;
        private Task _currentTask;
        private Func<Task> _nextTaskFunction;
        private List<Task> _regenerateTasks = new List<Task>(2);

        public override async Task AwaitGenerationProcess()
        {
            if (IsTaskCompleted())
            {
                return;
            }

            await _currentTask;
        }

        protected override void OnGenerationProcessStart()
        {
            _generationChunkPositionProvider.ChunkPositionChanged += ChunkPositionChanged;
            ChunkPositionChanged(_generationChunkPositionProvider.CalculateTargetLocalChunkPosition());
        }

        protected override void OnGenerationProcessStop()
        {
            _generationChunkPositionProvider.ChunkPositionChanged -= ChunkPositionChanged;
        }

        private void ChunkPositionChanged(Vector3Int chunkLocalPosition)
        {
            Debug.Log("Target chunk chnaged!");
            Vector3 globalChunkPosition = ChunkCoordinatesCalculator
                .GetGlobalChunkPositionByLocal(chunkLocalPosition);

            UpdateChunksVisibleArea(Vector3Int.FloorToInt(globalChunkPosition));
        }

        private async Task RegenerateArea(IEnumerable<Vector3Int> generateArea, IEnumerable<Vector3Int> disposeArea)
        {
            if (disposeArea != null)
            {
                _regenerateTasks.Add(DisposeArea(disposeArea, _disposeBatchLength, _disposeBatchDelay));
            }

            _regenerateTasks.Add(GenerateArea(generateArea));

            await Task.WhenAll(_regenerateTasks);
            _regenerateTasks.Clear();
        }

        private async Task GenerateArea(IEnumerable<Vector3Int> generateArea)
        {
            foreach (Vector3Int position in generateArea) 
            {
                await GenerateChunksBatch(position, position + Vector3Int.one);
            }
        }

        private void AddTaskFunctionToQueue(Func<Task> generationTaskFunction, Action currentTaskDoneCallback = null)
        {
            _nextTaskFunction = generationTaskFunction;
            InvokeNextTaskFunction(currentTaskDoneCallback);
        }

        private void InvokeNextTaskFunction(Action currentTaskDoneCallback = null)
        {
            if (IsTaskCompleted() && _nextTaskFunction != null)
            {
                _currentTask = _nextTaskFunction();
                _currentTask.ContinueWith(task => { currentTaskDoneCallback?.Invoke(); InvokeNextTaskFunction(); });
                _nextTaskFunction = null;
            }
        }

        private bool IsTaskCompleted()
        {
            return _currentTask == null || _currentTask.IsCompleted;
        }

        private void UpdateChunksVisibleArea(Vector3Int visibleAreaCenter)
        {
            if (!IsTaskCompleted())
            {
                return;
            }

            Vector3Int visibleAreaAcnhor = visibleAreaCenter - _viewDistance * Vector3Int.one;

            ShapeIntArea<SphereInt> currentViewShape = new ShapeIntArea<SphereInt>(
                new SphereInt(_viewDistance), visibleAreaAcnhor);

            Vector3Int delta = Vector3Int.zero; 
            if (_previousViewShape != null)
            {
                delta = _previousViewShape.Value.Anchor - currentViewShape.Anchor;
            }

            var debugText = $"Previous view pos updated: {currentViewShape.Anchor}, delta: {delta}";

            Action updatePreviousViewShape = () => { 
                Debug.Log(debugText); 
                _previousViewShape = currentViewShape; 
            };

            if (_previousViewShape == null)
            {
                AddTaskFunctionToQueue(() => RegenerateArea(currentViewShape.GetEveryPoint(), null), updatePreviousViewShape);
            }
            //else
            //{
            //    AddTaskFunctionToQueue(() => RegenerateArea(_loadArea.Value, _disposeArea.Value), updatePreviousViewShape);
            //}
        }
    }
}
