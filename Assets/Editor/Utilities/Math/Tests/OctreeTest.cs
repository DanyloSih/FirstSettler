﻿using NaughtyAttributes;
using Unity.Collections;
using UnityEngine;

namespace Utilities.Math.Tests
{
    public class OctreeTest : MonoBehaviour
    {
        [SerializeField] private Vector3Int _pointerPosition;
        [Range(0, 7)]
        [SerializeField] private int _pointerRank;
        [SerializeField] private int _pointerValue = 1;
        [SerializeField] private float _scale = 1f;
        [SerializeField] private int _applyArrayDelay = 1;

        private NativeOctree<int> _octree;

        protected void Start ()
        {
            _octree = new NativeOctree<int>(0, OctreeRank.Rank4);
        }

        protected void OnDestroy()
        {
            _octree.Dispose();
        }

        public void SetData()
        {
            _octree.Insert(OctreeNode<int>.FromGlobalPosition(_pointerValue, _pointerRank, _pointerPosition));
        }

        public void GetData()
        {
            if (_octree.TryGetNodeByGlobalPosition(_pointerPosition, out var node))
            {
                Debug.Log($"Data: {node.Data}, Rank: {node.Rank}, RankPosition: {node.RankPosition}");
            }
        }

        public async void ApplyArray()
        {
            ThreedimensionalNativeArray<int> sinArray = GenerateSineArray(Vector3Int.one * 16, 2, 1, 0);
            await _octree.SetDataFromArray(
                sinArray, delayInMilliseconds: _applyArrayDelay, cancellationToken: _octree.DisposingCancellationToken);
            sinArray.Dispose();
        }

        protected void OnDrawGizmosSelected()
        {
            if(!_octree.IsInitialized)
            {
                return;
            }

            Vector3 position = transform.position;

            NativeArray<OctreeNode<int>> nodes = _octree.GetNodes(Allocator.Temp);
            if (nodes.Length == 0) 
            {
                DrawWireCube(position, _octree.MaxRank, Color.red);
            }
            else
            {
                for (int i = 0; i < nodes.Length; i++)
                {
                    OctreeNode<int> node = nodes[i];
                    DrawWireCube(node.GlobalPosition + position, node.Rank, node.Data % 2 == 0 ? Color.red : Color.green);
                }
            }
            nodes.Dispose();

            Vector3Int pointerPos = NativeOctree<int>.RoundVectorToRank(_pointerRank, _pointerPosition);
            DrawWireCube(position + pointerPos, _pointerRank, Color.blue);
        }

        private ThreedimensionalNativeArray<int> GenerateSineArray(Vector3Int size, int waveAmplitude, int valueA, int valueB)
        {
            ThreedimensionalNativeArray<int> result = new ThreedimensionalNativeArray<int>(size);
            int midHeight = size.y / 2;
            RectPrismInt dataModel = result.RectPrism;
            for (int i = 0; i < dataModel.Volume; i++)
            {
                Vector3Int point = dataModel.IndexToPoint(i);
                float threshold = midHeight + Mathf.Sin(point.x / 2f) * waveAmplitude;
                //float threshold = midHeight;
                int value = point.y < threshold ? valueA : valueB;
                result.SetValue(point.x, point.y, point.z, value);
            }
            return result;
        }

        private void DrawWireCube(Vector3 position, int rank, Color color)
        {
            int dimensionSize = 1 << rank;
            Vector3 size = Vector3.one * (dimensionSize * _scale);
            Vector3 center = size / 2 + position * _scale;
            Gizmos.color = color;
            Gizmos.DrawWireCube(center, size);
        }
    }
}