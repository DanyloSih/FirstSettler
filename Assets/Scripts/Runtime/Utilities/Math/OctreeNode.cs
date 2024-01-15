using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utilities.Math
{
    public struct OctreeNode<T>
        where T : unmanaged
    {
        public T Data;

        private readonly int _rank;
        private readonly Vector3Int _position;

        /// <summary>
        /// Left Bottom Backward (min) position of octree node.
        /// </summary>
        public Vector3Int Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _position;
        }
        /// <summary>
        /// Determines how many steps to the left you need to 
        /// shift the bits of the number 1 to get the "<see cref="Size"/>". <br/>
        /// It also determines the maximum degree of nesting of "<see cref="OctreeNode"/>s". <br/>
        /// For example rank = 4: <br/>
        /// size = 1 &lt;&lt; 4; <br/>
        /// size == 16; <br/>
        /// Nesting struct: Node4 -&gt; Node3 -&gt; Node2 -&gt; Node1 -&gt; Node0;
        /// </summary>
        public int Rank
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _rank;
        }

        public OctreeNode(T data, int rank, Vector3Int position)
        {
            Data = data;
            _rank = rank;
            _position = position;
        }
    }
}