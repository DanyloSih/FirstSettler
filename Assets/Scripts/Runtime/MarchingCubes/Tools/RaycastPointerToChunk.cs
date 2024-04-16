using UnityEngine;
using World.Organization;
using World.Data;
using Zenject;

namespace MarchingCubesProject.Tools
{
    public class RaycastPointerToChunk : MonoBehaviour
    {
        [SerializeField] private Camera _rayThrowerCamera;
        [SerializeField] private float _maxRaycastDistance;

        private ChunkCoordinatesCalculator _chunkCoordinatesCalculator;
        private BasicChunkSettings _basicChunkSettings;

        [Inject]
        public void Construct(BasicChunkSettings basicChunkSettings)
        {
            _basicChunkSettings = basicChunkSettings;
        }

        protected void OnEnable()
        {
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
                var chunk = hit.collider.transform.parent.GetComponent<MarchingCubesChunk>();
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
