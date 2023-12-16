using System.Collections.Generic;
using UnityEngine;
using World.Data;
using World.Organization;

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
                ThrowRayAndDeform(Mathf.Abs(_deformForce));
            }
            else if (Input.GetMouseButtonDown(1))
            {
                ThrowRayAndDeform(0);
            }
            else if (Input.GetMouseButtonDown(2))
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

        private void Deform(Vector3 globalChunkPosition, float scale, float volume, int materialHash)
        {
            List<ChunkDataVolumeAndMaterial> changePoints = new List<ChunkDataVolumeAndMaterial>();
            int halfBrushSize = _brushSize / 2;
            for (int x = -halfBrushSize; x <= halfBrushSize; x++) 
            {
                for (int y = -halfBrushSize; y <= halfBrushSize; y++)
                {
                    for (int z = -halfBrushSize; z <= halfBrushSize; z++)
                    {
                        var brushPoint = globalChunkPosition + new Vector3(x, y, z) * scale;

                        if (Vector3.Distance(globalChunkPosition, brushPoint) <= _brushSize)
                        {
                            changePoints.Add(new ChunkDataVolumeAndMaterial(
                                brushPoint, volume, materialHash));
                        }
                    }
                }
            }
            _marchingCubesChunksEditor.SetNewChunkDataVolumeAndMaterial(changePoints);
        }

        private void SetVolume(IChunk chunk, Vector3Int voxelPosInt, float volume)
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
