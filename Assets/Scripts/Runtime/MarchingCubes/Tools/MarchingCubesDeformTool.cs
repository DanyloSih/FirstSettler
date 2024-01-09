using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FirstSettler.Extensions;
using UnityEngine;
using Utilities.Math;
using World.Data;

namespace MarchingCubesProject.Tools
{
    public class MarchingCubesDeformTool : MonoBehaviour
    {
        [SerializeField] private RaycastPointerToChunk _raycastPointerToChunk;
        [SerializeField] [Range(0, 1)] private float _deformForce = 0.2f;
        [SerializeField] private int _brushSize = 2;
        [SerializeField] private MaterialKey _drawMaterial;
        [SerializeField] private MarchingCubesChunksEditor _marchingCubesChunksEditor;
        [SerializeField] private BasicChunkSettings _basicChunkSettings;

        private Stopwatch _cooldownStopwatch = new Stopwatch();
        private int _drawMaterialHash;
        private Camera _mainCamera;
        List<ChunkPoint> _changePoints = new List<ChunkPoint>();

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
            if (Input.GetMouseButtonDown(1))
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
            Vector3Int chunkSize = _basicChunkSettings.Size;
            int halfBrushSize = _brushSize / 2;
            ChunkPoint initialDataPoint = _marchingCubesChunksEditor
                .GetChunkDataPoint(globalChunkDataPoint);

            if (initialDataPoint == default)
            {
                return;
            }

            Vector3Int offset = -Vector3Int.one * halfBrushSize;
            Vector3Int unscaledGlobalChunkPosition = Vector3Int.Scale(
                initialDataPoint.LocalChunkPosition.FloorToVector3Int(), chunkSize);

            Vector3Int localChunkDataPoint = initialDataPoint.LocalChunkDataPoint.FloorToVector3Int();
            Vector3Int unscaledGlobalDataPoint = unscaledGlobalChunkPosition + localChunkDataPoint;
                


            Parallelepiped editingParallelepiped = new Parallelepiped(Vector3Int.one * _brushSize);
            Area editingArea = new Area(editingParallelepiped, unscaledGlobalDataPoint);

            for (int i = 0; i < editingParallelepiped.Volume; i++)
            {
                Vector3Int pointerInArea = editingParallelepiped.IndexToVoxelPosition(i) + offset;
                float unscaledDistance = pointerInArea.magnitude;
                float deformForce = unscaledDistance / halfBrushSize;
                
                if (unscaledDistance < halfBrushSize)
                {
                    Vector3Int globalUnscaledDataPointPointer = unscaledGlobalDataPoint + pointerInArea;
                    Vector3Int pointedChunk = globalUnscaledDataPointPointer.GetElementwiseDividedVector(chunkSize);
                    Vector3Int pointedChunkData = globalUnscaledDataPointPointer.GetElementwiseDividingRemainder(chunkSize);
                    Vector3Int fixedPointedChunkData = FixNegativePoint(pointedChunkData, chunkSize, out var chunkOffset);
                    pointedChunk += chunkOffset;

                    ChunkPoint chunkDataPoint
                        = _marchingCubesChunksEditor.GetChunkDataPoint(pointedChunk, fixedPointedChunkData);

                    if (chunkDataPoint == default)
                    {
                        continue;
                    }

                    chunkDataPoint.Volume = Mathf.Clamp01(deformFactor);
                    if (chunkDataPoint.Volume > 0)
                    {
                        chunkDataPoint.MaterialHash = materialHash;
                    }
                    _changePoints.Add(chunkDataPoint);
                }  
            }

            await _marchingCubesChunksEditor.SetVoxels(_changePoints, editingArea);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector3Int FixNegativePoint(Vector3Int point, Vector3Int chunkSize, out Vector3Int chunkOffset)
        {
            chunkOffset = new Vector3Int();
            if (point.x < 0)
            {
                point.x = chunkSize.x + point.x;
                chunkOffset.x = -1;
            }

            if (point.y < 0)
            {
                point.y = chunkSize.y + point.y;
                chunkOffset.y = -1;
            }

            if (point.z < 0)
            {
                point.z = chunkSize.z + point.z;
                chunkOffset.z = -1;
            }

            return point;
        }
    }
}
