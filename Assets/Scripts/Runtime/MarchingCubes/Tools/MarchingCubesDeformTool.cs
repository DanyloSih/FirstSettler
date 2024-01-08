using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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

        private Stopwatch _cooldownStopwatch = new Stopwatch();
        private int _drawMaterialHash;
        private Camera _mainCamera;
        List<VoxelBlueprint> _changePoints = new List<VoxelBlueprint>();

        protected void OnEnable()
        {
            if (_drawMaterial != null)
            {
                _drawMaterialHash = _drawMaterial.GetHashCode();
            }
            _mainCamera = Camera.main;
        }

        protected async void Update()
        {
            if (Input.GetMouseButton(1))
            {
                await ThrowRayAndDeform(Mathf.Abs(_deformForce));
            }
            else if (Input.GetMouseButtonDown(2))
            {
                await ThrowRayAndDeform(0);
            }
            else if (Input.GetMouseButtonDown(0))
            {
                await ThrowRayAndDeform(-Mathf.Abs(_deformForce));
            }
        }

        private async Task ThrowRayAndDeform(float newVolume)
        {
            if (_marchingCubesChunksEditor.IsAlreadyEditingChunks())
            {
                return;
            }

            ChunkRaycastingResult chunkRaycastResult = _raycastPointerToChunk.ThrowRaycast();

            if (!chunkRaycastResult.IsChunkHited)
            {
                return;
            }

            await Deform(chunkRaycastResult.GlobalChunkDataPoint, chunkRaycastResult.Scale, newVolume, _drawMaterial.GetHashCode());
        }

        private async Task Deform(Vector3 globalChunkDataPoint, float scale, float deformFactor, int materialHash)
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
                        VoxelBlueprint chunkDataPoint 
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

            await _marchingCubesChunksEditor.SetVoxels(_changePoints, _changePoints.Count);
        }
    }
}
