using System.Collections;
using UnityEngine;
using Utilities.Math;

namespace World.Organization
{
    public class DynamicChunksGridGenerator : ChunksGeneratorBase
    {
        [SerializeField] private int _viewDistance = 16;

        private Camera _mainCamera;
        private Transform _mainCameraTransform;
        private Coroutine _visibleAreaChangesChekerCoroutine;
        private Vector3Int _previousCameraChunkPosition;
        private ShapeIntArea<SphereInt>? _previousViewShape;

        protected override void InitializeChunks()
        {
            _mainCamera = Camera.main;
            _mainCameraTransform = _mainCamera.transform;
            _previousCameraChunkPosition = ChunkCoordinatesCalculator
                    .GetLocalChunkPositionByGlobalPoint(_mainCameraTransform.position);

            _visibleAreaChangesChekerCoroutine = StartCoroutine(
                ChunksVisibleAreaChangesCheckerProcess());      
        }

        protected void OnDisable()
        {
            StopCoroutine(_visibleAreaChangesChekerCoroutine);
        }

        private void UpdateChunksVisibleArea(Vector3Int visibleAreaCenter)
        {
            Vector3Int visibleAreaAcnhor = visibleAreaCenter - _viewDistance * Vector3Int.one;

            ShapeIntArea<SphereInt> currentViewShape = new ShapeIntArea<SphereInt>(
                new SphereInt(_viewDistance), visibleAreaAcnhor);

            if(_previousViewShape == null)
            {

            }

            _previousViewShape = currentViewShape;
        }

        private IEnumerator ChunksVisibleAreaChangesCheckerProcess()
        {
            UpdateChunksVisibleArea(ChunkCoordinatesCalculator
                    .GetLocalChunkPositionByGlobalPoint(_mainCameraTransform.position));

            yield return new WaitForEndOfFrame();

            while (true)
            {
                Vector3Int cameraChunkPosition = ChunkCoordinatesCalculator
                    .GetLocalChunkPositionByGlobalPoint(_mainCameraTransform.position);

                if(cameraChunkPosition != _previousCameraChunkPosition)
                {
                    UpdateChunksVisibleArea(cameraChunkPosition);
                }

                _previousCameraChunkPosition = cameraChunkPosition;
                yield return new WaitForEndOfFrame();  
            }   
        }
    }
}
