using System.Collections.Generic;
using UnityEngine;
using World.Data;

namespace MarchingCubesProject.Tools
{
    public class MarchingCubesDeformTool : MonoBehaviour
    {
        [SerializeField] private RaycastPointerToChunk _raycastPointerToChunk;
        [SerializeField] private float _deformForce = 0.2f;
        [SerializeField] private int _brushSize = 2;
        [SerializeField] private MaterialKey _drawMaterial;
        [SerializeField] private MarchingCubesChunksEditor _marchingCubesChunksEditor;

        private int _drawMaterialHash;
        private Camera _mainCamera;
        List<ChunkDataPoint> _changePoints = new List<ChunkDataPoint>();

        private void OnEnable()
        {
            if (_drawMaterial != null)
            {
                _drawMaterialHash = _drawMaterial.GetHashCode();
            }
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                ThrowRayAndDeform(Mathf.Abs(_deformForce));
            }
            else if (Input.GetMouseButtonDown(2))
            {
                ThrowRayAndDeform(0);
            }
            else if (Input.GetMouseButtonDown(0))
            {
                ThrowRayAndDeform(-Mathf.Abs(_deformForce));
            }
        }

        private void ThrowRayAndDeform(float newVolume)
        {
            ChunkRaycastingResult chunkRaycastResult = _raycastPointerToChunk.ThrowRaycast();

            if (!chunkRaycastResult.IsChunkHited)
            {
                return;
            }

            Deform(chunkRaycastResult.GlobalChunkDataPoint, chunkRaycastResult.Scale, newVolume, _drawMaterial.GetHashCode());
        }

        private void Deform(Vector3 globalChunkDataPoint, float scale, float deformFactor, int materialHash)
        {
            _changePoints.Clear();
            int halfBrushSize = _brushSize / 2;
            for (int x = -halfBrushSize; x <= halfBrushSize; x++) 
            {
                for (int y = -halfBrushSize; y <= halfBrushSize; y++)
                {
                    for (int z = -halfBrushSize; z <= halfBrushSize; z++)
                    {
                        Vector3 brushPointScaled = globalChunkDataPoint + new Vector3(x, y, z) * scale;
                        Vector3 brushPointUnscaled = globalChunkDataPoint + new Vector3(x, y, z);
                        ChunkDataPoint chunkDataPoint 
                            = _marchingCubesChunksEditor.GetChunkDataPoint(brushPointScaled);
                        
                        float distance = Vector3.Distance(globalChunkDataPoint, brushPointUnscaled);
                        float brushForce = distance == 0 ? 1 : 1f - Mathf.Clamp01(distance / halfBrushSize);
                        float resultVolume = Mathf.Clamp01(chunkDataPoint.Volume + deformFactor * brushForce);

                        if (chunkDataPoint.Volume != resultVolume)
                        {
                            chunkDataPoint.Volume = resultVolume;
                            _changePoints.Add(chunkDataPoint);
                        }
                    }
                }
            }
            _marchingCubesChunksEditor.SetNewChunkDataVolumeAndMaterial(_changePoints);
        }
    }
}
