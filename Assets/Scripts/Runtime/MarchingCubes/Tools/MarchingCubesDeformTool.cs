using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Utilities.Math;
using Utilities.Threading;
using World.Data;
using World.Organization;
using Zenject;

namespace MarchingCubesProject.Tools
{
    public class MarchingCubesDeformTool : MonoBehaviour
    {
        [SerializeField] private RaycastPointerToChunk _raycastPointerToChunk;
        [SerializeField] [Range(0, 1)] private float _deformForce = 0.2f;
        [SerializeField] private int _brushSize = 2;
        [SerializeField] private MaterialKey _drawMaterial;

        private Stopwatch _cooldownStopwatch = new Stopwatch();
        private int _drawMaterialHash;
        private Camera _mainCamera;
        private IChunksContainer _chunksContainer;
        private BasicChunkSettings _basicChunkSettings;
        private MarchingCubesChunksEditor _marchingCubesChunksEditor;

        [Inject]
        public void Construct(
            BasicChunkSettings basicChunkSettings, 
            MarchingCubesChunksEditor marchingCubesChunksEditor,
            IChunksContainer chunksContainer)
        {
            _basicChunkSettings = basicChunkSettings;
            _marchingCubesChunksEditor = marchingCubesChunksEditor;
            _chunksContainer = chunksContainer;
        }

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

            if (!chunkRaycastResult.IsChunkHit)
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
                Vector3Int.FloorToInt(initialDataPoint.LocalChunkPosition), chunkSize);

            Vector3Int localChunkDataPoint = Vector3Int.FloorToInt(initialDataPoint.LocalChunkDataPoint);
            Vector3Int unscaledGlobalDataPoint = unscaledGlobalChunkPosition + localChunkDataPoint;            

            RectPrismInt editingPrism = new RectPrismInt(Vector3Int.one * _brushSize);
            ShapeIntArea<RectPrismInt> editingArea = new ShapeIntArea<RectPrismInt>(
                editingPrism, unscaledGlobalDataPoint - editingPrism.HalfSize);

            NativeList<ChunkPoint> chunkPoints = new NativeList<ChunkPoint>(editingPrism.Volume, Allocator.Persistent);
            NativeHashMap<int, IntPtr> chunksDataPointersInsideEditArea 
                = ChunksMath.GetChunksDataPointersInsideArea(editingArea, chunkSize, _chunksContainer);

            DeformMaskJob deformMaskJob = new DeformMaskJob();
            deformMaskJob.DeformFactor = deformFactor;
            deformMaskJob.ChunkDataModel = new RectPrismInt(chunkSize + Vector3Int.one);
            deformMaskJob.ChunksDataPointersInsideEditArea = chunksDataPointersInsideEditArea;
            deformMaskJob.ChunkSize = chunkSize;
            deformMaskJob.EditingPrism = editingPrism;
            deformMaskJob.HalfBrushSize = halfBrushSize;
            deformMaskJob.MaterialHash = materialHash;
            deformMaskJob.Offset = offset;
            deformMaskJob.UnscaledGlobalDataPoint = unscaledGlobalDataPoint;
            deformMaskJob.ChunkPoints = chunkPoints.AsParallelWriter();

            JobHandle deformMaskJobHandler = deformMaskJob.Schedule(editingArea.Shape.Volume, 1);
            await AsyncUtilities.WaitWhile(() => !deformMaskJobHandler.IsCompleted);
            deformMaskJobHandler.Complete();

            await _marchingCubesChunksEditor.SetVoxels(
                chunkPoints.AsArray(),
                chunkPoints.Length, 
                chunksDataPointersInsideEditArea);

            chunkPoints.Dispose();
            chunksDataPointersInsideEditArea.Dispose();
        }
    }
}
