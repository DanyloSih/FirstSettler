using UnityEngine;
using World.Data;
using Zenject;
using Utilities.Math;

namespace MarchingCubesProject.Tools
{
    public class RaycastPointerToChunk : MonoBehaviour
    {
        [SerializeField] private float _maxRaycastDistance;

        private Camera _rayThrowerCamera;
        private ChunkCoordinatesCalculator _chunkCoordinatesCalculator;
        private BasicChunkSettings _basicChunkSettings;

        [Inject]
        public void Construct(BasicChunkSettings basicChunkSettings)
        {
            _basicChunkSettings = basicChunkSettings;
        }

        protected void OnEnable()
        {
            _rayThrowerCamera = Camera.main;
            _chunkCoordinatesCalculator = new ChunkCoordinatesCalculator(
                _basicChunkSettings.Size, _basicChunkSettings.Scale);
        }

        public ChunkRaycastingResult ThrowRaycast()
        {
            var result = new ChunkRaycastingResult(); 
            var ray = _rayThrowerCamera.ScreenPointToRay(
                new Vector3(Screen.width / 2, Screen.height / 2, 0));

            result.Ray = ray;

            if (Physics.Raycast(ray, out var hit, _maxRaycastDistance))
            {
                result.Hit = hit;
                var chunk = hit.collider.transform.parent.GetComponent<Chunk>();
                if (chunk == null)
                {
                    return result;
                }

                result.Chunk = chunk;
                result.GlobalChunkDataPoint = _chunkCoordinatesCalculator
                    .GetGlobalChunkDataPointByGlobalPoint(hit.point);

                result.LocalChunkDataPoint = _chunkCoordinatesCalculator
                    .GetLocalChunkDataPointByGlobalPoint(hit.point);

                result.Scale = _basicChunkSettings.Scale;
                result.IsChunkHit = true;
            }

            return result;
        }
    }
}
