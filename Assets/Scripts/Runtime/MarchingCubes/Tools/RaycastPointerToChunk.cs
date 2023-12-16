using UnityEngine;
using FirstSettler.Common;

namespace MarchingCubesProject.Tools
{
    public class RaycastPointerToChunk : MonoBehaviour
    {
        [SerializeField] private CommonRaycastPointer _commonRaycastPointer;

        private MeshFilter _currentPointedMeshFilter;
        private MarchingCubesBasicChunk _currentPointedChunk;
        private Vector3Int _currentPointedChunkDataPosition;

        public CommonRaycastPointer CommonRaycastPointer { get => _commonRaycastPointer; }
        public MeshFilter CurrentPointedMeshFilter { get => _currentPointedMeshFilter; }
        public MarchingCubesBasicChunk CurrentPointedChunk { get => _currentPointedChunk; }
        /// <summary>
        /// Coordinates in ChunkData pointed by ray casted 
        /// from the center of the camera.
        /// </summary>
        public Vector3Int CurrentPointedPositionInChunkData { get => _currentPointedChunkDataPosition; }

        protected void OnEnable()
        {
            _commonRaycastPointer.RaycastHitsUpdated += OnRaycastHitsUpdated;
        }

        protected void OnDisable()
        {
            _commonRaycastPointer.RaycastHitsUpdated -= OnRaycastHitsUpdated;
        }

        private void OnRaycastHitsUpdated(RaycastHit[] hits)
        {
            foreach (var hit in hits)
            {
                var chunk = hit.collider.transform.parent.GetComponent<MarchingCubesBasicChunk>();
                if (chunk == null)
                {
                    _currentPointedMeshFilter = null;
                    _currentPointedChunk = null;
                    return;
                }

                var meshFilter = hit.collider.transform.GetComponent<MeshFilter>();

                _currentPointedMeshFilter = meshFilter;
                _currentPointedChunk = chunk;

                var chunkSize = chunk.ChunkSize;
                var chunkExtendMinusOne = chunkSize / 2;

                Vector3 offset = meshFilter.transform.parent.position - Vector3.one / 2;
                Vector3 hitOffset = hit.point - offset + chunkExtendMinusOne;
                Vector3 normalizedHitPoint = new Vector3(
                        hitOffset.x / chunkSize.x,
                        hitOffset.y / chunkSize.y,
                        hitOffset.z / chunkSize.z);

                var voxelPos = Vector3.Scale(chunkSize, normalizedHitPoint);
                var voxelPosInt = new Vector3Int(
                    Mathf.FloorToInt(voxelPos.x), 
                    Mathf.FloorToInt(voxelPos.y), 
                    Mathf.FloorToInt(voxelPos.z));

                _currentPointedChunkDataPosition = voxelPosInt;
            }
        } 
    }
}
