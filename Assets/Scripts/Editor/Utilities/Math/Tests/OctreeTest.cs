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
        [SerializeField] private bool _applyTrigger = false;
        [SerializeField] private float _scale = 1f;

        private NativeOctree<int> _octree;

        protected void Start ()
        {
            _octree = new NativeOctree<int>(0, OctreeRank.Rank4);
        }

        protected void OnDestroy()
        {
            _octree.Dispose();
        }

        protected void OnValidate()
        {
            if (_applyTrigger == true)
            {
                if (!_octree.IsInitialized)
                {
                    return;
                }
                _octree.SetData(_pointerValue, _pointerRank, _pointerPosition);
                _applyTrigger = false;
            }
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
                    DrawWireCube(node.Position + position, node.Rank, node.Rank % 2 == 0 ? Color.red : Color.green);
                }
            }
            nodes.Dispose();

            Vector3Int pointerPos = NativeOctree<int>.RoundVectorToRank(_pointerRank, _pointerPosition);
            DrawWireCube(position + pointerPos, _pointerRank, Color.blue);
        }

        private void DrawWireCube(Vector3 position, int rank, Color color)
        {
            int dimensionSize = 1 << rank;
            Vector3 size = Vector3.one * (dimensionSize * _scale);
            Vector3 center = size / 2 + position;
            Gizmos.color = color;
            Gizmos.DrawWireCube(center, size);
        }
    }
}