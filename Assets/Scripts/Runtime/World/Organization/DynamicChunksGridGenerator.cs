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

        protected override void InitializeChunks()
        {
            _mainCamera = Camera.main;
            _mainCameraTransform = _mainCamera.transform;
            var viewSphere = new SphereInt(25000);
        }

        private void ChunksVisibleAreaUpdated(RectPrismAreaInt visibleArea)
        {
            
        }

        private IEnumerator ChunksVisibleAreaChangesCheckerProcess()
        {
            while (true)
            {
                Vector3Int cameraChunkPosition = ChunkCoordinatesCalculator
                    .GetLocalChunkPositionByGlobalPoint(_mainCameraTransform.position);

                

                yield return new WaitForEndOfFrame();  
            }   
        }
    }
}
