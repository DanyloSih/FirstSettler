﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            if (!initialDataPoint.IsInitialized)
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
            deformMaskJob.DeformFactor = deformFactor;
            deformMaskJob.ChunkDataModel = new Parallelepiped(chunkSize + Vector3Int.one);
            deformMaskJob.ChunksDataPointersInsideEditArea = chunksDataPointersInsideEditArea;
            deformMaskJob.ChunkSize = chunkSize;
            deformMaskJob.EditingParallelepiped = editingParallelepiped;
            deformMaskJob.HalfBrushSize = halfBrushSize;
            deformMaskJob.MaterialHash = materialHash;
            deformMaskJob.Offset = offset;
            deformMaskJob.UnscaledGlobalDataPoint = unscaledGlobalDataPoint;
            deformMaskJob.ChunkPoints = chunkPoints.AsParallelWriter();

            var deformMaskJobHandler = deformMaskJob.Schedule(editingArea.Parallelepiped.Volume, 1);
            deformMaskJobHandler.Complete();

            List<int> debugMat = new List<int>();
            foreach (var point in chunkPoints)
            {
                if (point.MaterialHash != 0 && point.MaterialHash != materialHash)
                {
                    debugMat.Add(point.MaterialHash);
                }
            }

            await _marchingCubesChunksEditor.SetVoxels(
                chunkPoints.AsArray(),
                chunkPoints.Length, 
                chunksDataPointersInsideEditArea);
        }
    }
}
