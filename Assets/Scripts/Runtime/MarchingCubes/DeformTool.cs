using System.Collections.Generic;
using UnityEngine;

namespace MarchingCubesProject
{
    public class DeformTool : MonoBehaviour
    {
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
                Ray ray = _mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                if (Physics.Raycast(ray, out var hit, 100))
                {
                    var meshFilter = hit.collider.transform.GetComponent<MeshFilter>();
                    if (meshFilter == null) return;

                    var chunk = meshFilter.transform.parent.GetComponent(typeof(IChunk)) as IChunk;
                    if (chunk != null)
                    {
                        var chunkSize = chunk.BasicChunkSettings.ChunkSize;
                        var chunkSizeMinusOne = chunkSize - Vector3.one;
                        var chunkExtendMinusOne = chunkSizeMinusOne / 2;
                        var mesh = meshFilter.mesh;
                        Vector3 offset = meshFilter.transform.parent.position - Vector3.one / 2;
                        Vector3 hitOffset = hit.point - offset + chunkExtendMinusOne;
                        Vector3 normalizedHitPoint = new Vector3(
                                hitOffset.x / chunkSizeMinusOne.x,
                                hitOffset.y / chunkSizeMinusOne.y,
                                hitOffset.z / chunkSizeMinusOne.z);

                        var voxelPos = Vector3.Scale(chunkSize, normalizedHitPoint);
                        var voxelPosInt = new Vector3Int(Mathf.FloorToInt(voxelPos.x), Mathf.FloorToInt(voxelPos.y), Mathf.FloorToInt(voxelPos.z));
                        Deform(chunk, voxelPosInt);
                        foreach (IChunk neighborChunk in chunk.Neighbors.Neighbors)
                        {
                            if (neighborChunk != null)
                            {
                                neighborChunk.UpdateMesh();
                            }
                        }
                        
                    }
                }

            }
        }

        private void Deform(IChunk chunk, Vector3Int voxelPosInt)
        {
            var chunkSize = chunk.BasicChunkSettings.ChunkSize;
            var chunkSizeMinusOne = chunkSize - Vector3Int.one;

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
                            Mathf.Clamp(brushPoint.x, 0, chunkSizeMinusOne.x),
                            Mathf.Clamp(brushPoint.y, 0, chunkSizeMinusOne.y),
                            Mathf.Clamp(brushPoint.z, 0, chunkSizeMinusOne.z));

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
