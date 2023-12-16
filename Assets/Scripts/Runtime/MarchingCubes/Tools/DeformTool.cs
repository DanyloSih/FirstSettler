using System.Collections.Generic;
using UnityEngine;
using World.Data;

namespace MarchingCubesProject.Tools
{
    public class DeformTool : MonoBehaviour
    {
        [SerializeField] private RaycastPointerToChunk _raycastPointerToChunk;
        [SerializeField] private float _deformForce = 0.2f;
        [SerializeField] private int _brushSize = 2;
        [SerializeField] private MaterialKey _drawMaterial;

        private int _drawMaterialHash;
        private Camera _mainCamera;

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
            if (Input.GetMouseButtonDown(0))
            {
                var chunk = _raycastPointerToChunk.CurrentPointedChunk;

                if (chunk == null)
                {
                    return;
                }

                var voxelPosInt = _raycastPointerToChunk.CurrentPointedPositionInChunkData;
                Deform(chunk, voxelPosInt);
                chunk.UpdateMesh();
            }
        }

        private void Deform(MarchingCubesBasicChunk chunk, Vector3Int voxelPosInt)
        {
            var chunkSize = chunk.ChunkSize;

            List<Vector3Int> changePoints = new List<Vector3Int>();
            int halfBrushSize = _brushSize / 2;
            for (int x = -halfBrushSize; x <= halfBrushSize; x++) 
            {
                for (int y = -halfBrushSize; y <= halfBrushSize; y++)
                {
                    for (int z = -halfBrushSize; z <= halfBrushSize; z++)
                    {
                        var brushPoint = voxelPosInt + new Vector3Int(x, y, z);
                        var clampedBrushPoint = new Vector3Int(
                            Mathf.Clamp(brushPoint.x, 0, chunkSize.x),
                            Mathf.Clamp(brushPoint.y, 0, chunkSize.y),
                            Mathf.Clamp(brushPoint.z, 0, chunkSize.z));

                        if (clampedBrushPoint == brushPoint 
                         && Vector3Int.Distance(voxelPosInt, brushPoint) <= _brushSize)
                        {
                            changePoints.Add(brushPoint);
                        }
                    }
                }
            }
            foreach (var changePoint in changePoints)
            {
                SetVolume(chunk, changePoint, chunk.ChunkData.GetVolume(changePoint.x, changePoint.y, changePoint.z));
            }
        }

        private void SetVolume(MarchingCubesBasicChunk chunk, Vector3Int voxelPosInt, float volume)
        {
            if (volume >= 0 && _drawMaterial != null)
            {
                chunk.ChunkData.SetMaterialHash(
                    voxelPosInt.x,
                    voxelPosInt.y,
                    voxelPosInt.z,
                    _drawMaterialHash);
            }

            chunk.ChunkData.SetVolume(
                voxelPosInt.x,
                voxelPosInt.y,
                voxelPosInt.z,
                Mathf.Clamp01(volume + _deformForce));
        }
    }
}
