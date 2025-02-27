﻿using UnityEngine;

namespace SimpleChunks.Tools
{
    public class TargetChunkDataPointMarker : MonoBehaviour
    {
        [SerializeField] private Transform _markerTransform;
        [SerializeField] private RaycastPointerToChunk _raycastPointerToChunk;

        protected void OnEnable()
        {
            if (_markerTransform != null && !_markerTransform.Equals(null))
            {
                _markerTransform.gameObject.SetActive(true);
            }
        }

        protected void OnDisable()
        {
            if (_markerTransform != null && !_markerTransform.Equals(null))
            {
                _markerTransform.gameObject.SetActive(false);
            }
        }

        protected void Update()
        {
            ChunkRaycastingResult chunkRaycastResult = _raycastPointerToChunk.ThrowRaycast();

            if (!chunkRaycastResult.IsChunkHit)
            {
                return;
            }

            _markerTransform.position = chunkRaycastResult.GlobalChunkDataPoint;
        }
    }
}
