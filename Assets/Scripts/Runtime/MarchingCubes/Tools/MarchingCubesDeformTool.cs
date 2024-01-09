using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FirstSettler.Extensions;
using SimpleHeirs;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Utilities.Math;
using World.Data;
using World.Organization;

namespace MarchingCubesProject.Tools
{
    public struct DeformMaskJob : IJobParallelFor
    {
        [ReadOnly] public Vector3Int offset;
        [ReadOnly] public Vector3Int unscaledGlobalDataPoint;
        [ReadOnly] public Parallelepiped editingParallelepiped;
        [ReadOnly] public Parallelepiped chunkDataModel;
        [ReadOnly] public Vector3Int chunkSize;
        [ReadOnly] public int halfBrushSize;
        [ReadOnly] public float deformFactor;
        [ReadOnly] public int materialHash;
        [ReadOnly] public NativeHashMap<long, IntPtr> chunksDataPointersInsideEditArea;

        [WriteOnly] public NativeList<ChunkPoint>.ParallelWriter chunkPoints;
        [WriteOnly] public int itemsCount;

        public void Execute(int index)
        {
            Vector3Int pointerInArea = editingParallelepiped.IndexToVoxelPosition(index) + offset;
            float unscaledDistance = pointerInArea.magnitude;
            float deformForce = unscaledDistance / halfBrushSize;

            if (unscaledDistance < halfBrushSize)
            {
                Vector3Int globalUnscaledDataPointPointer = unscaledGlobalDataPoint + pointerInArea;
                Vector3Int pointedChunk = globalUnscaledDataPointPointer.GetElementwiseDividedVector(chunkSize);
                Vector3Int pointedChunkData = globalUnscaledDataPointPointer.GetElementwiseDividingRemainder(chunkSize);
                pointedChunkData = FixNegativePoint(pointedChunkData, chunkSize, out var chunkOffset);
                pointedChunk += chunkOffset;

                long chunkPositionHash = PositionHasher.GetPositionHash(
                    pointedChunk.x, pointedChunk.y, pointedChunk.z);

                if (!chunksDataPointersInsideEditArea.ContainsKey(chunkPositionHash))
                {
                    return;
                }

                IntPtr rawDataStartPointer = chunksDataPointersInsideEditArea[chunkPositionHash];
                int chunkVoxelOffset = chunkDataModel.VoxelPositionToIndex(
                    pointedChunk.x, pointedChunk.y, pointedChunk.z);

                VoxelData data;

                unsafe
                {
                    VoxelData* dataPointer = (VoxelData*)rawDataStartPointer.ToPointer();
                    data = dataPointer[chunkVoxelOffset];
                }

                if (data.MaterialHash == 0)
                {
                    return;
                }

                float volume = Mathf.Clamp01(deformFactor);
                int hash = data.MaterialHash;
                if (volume > 0)
                {
                    hash = materialHash;
                }
                chunkPoints.AddNoResize(new ChunkPoint(pointedChunk, pointedChunkData, volume, hash));
            }
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

    public class MarchingCubesDeformTool : MonoBehaviour
    {
        [SerializeField] private RaycastPointerToChunk _raycastPointerToChunk;
        [SerializeField] [Range(0, 1)] private float _deformForce = 0.2f;
        [SerializeField] private int _brushSize = 2;
        [SerializeField] private MaterialKey _drawMaterial;
        [SerializeField] private MarchingCubesChunksEditor _marchingCubesChunksEditor;
        [SerializeField] private BasicChunkSettings _basicChunkSettings;
        [SerializeField] private HeirsProvider<IChunksContainer> _chunksContainerHeir;

        private Stopwatch _cooldownStopwatch = new Stopwatch();
        private int _drawMaterialHash;
        private Camera _mainCamera;
        private IChunksContainer _chunksContainer;

        protected void OnEnable()
        {
            if (_drawMaterial != null)
            {
                _drawMaterialHash = _drawMaterial.GetHashCode();
            }
            _mainCamera = Camera.main;
            _chunksContainer = _chunksContainerHeir.GetValue();
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

            await Deform(chunkRaycastResult.GlobalChunkDataPoint, newVolume, _drawMaterial.GetHashCode());
        }

        private async Task Deform(Vector3 globalChunkDataPoint, float deformFactor, int materialHash)
        {
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

            NativeList<ChunkPoint> chunkPoints = new NativeList<ChunkPoint>(editingParallelepiped.Volume, Allocator.Persistent);
            NativeHashMap<long, IntPtr> chunksDataPointersInsideEditArea 
                = ChunksMath.GetChunksDataPointersInsideArea(editingArea, chunkSize, _chunksContainer);

            DeformMaskJob deformMaskJob = new DeformMaskJob();
            deformMaskJob.deformFactor = deformFactor;
            deformMaskJob.chunkDataModel = new Parallelepiped(chunkSize + Vector3Int.one);
            deformMaskJob.chunksDataPointersInsideEditArea = chunksDataPointersInsideEditArea;
            deformMaskJob.chunkSize = chunkSize;
            deformMaskJob.editingParallelepiped = editingParallelepiped;
            deformMaskJob.halfBrushSize = halfBrushSize;
            deformMaskJob.materialHash = materialHash;
            deformMaskJob.offset = offset;
            deformMaskJob.unscaledGlobalDataPoint = unscaledGlobalDataPoint;
            deformMaskJob.chunkPoints = chunkPoints.AsParallelWriter();

            var deformMaskJobHandler = deformMaskJob.Schedule(editingArea.Parallelepiped.Volume, 1);
            deformMaskJobHandler.Complete();


            await _marchingCubesChunksEditor.SetVoxels(
                chunkPoints.AsArray(),
                chunkPoints.Length, 
                chunksDataPointersInsideEditArea);
        }
    }
}
