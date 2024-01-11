using System.Runtime.CompilerServices;

namespace Utilities.Math
{
    public struct Octree<T>
        where T : struct
    {
        public struct OctreeNode
        {
            public T Data;

            private readonly int _rank;
            private readonly int _size;

            /// <summary>
            /// Determines how many steps to the left you need to 
            /// shift the bits of the number 1 to get the "<see cref="Size"/>". <br/>
            /// It also determines the maximum degree of nesting of "<see cref="OctreeNode"/>". <br/>
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
            /// <summary>
            /// Determines how many "elementary subnodes" (node with rank 0) can fit inside this node.
            /// </summary>
            public int Size
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _size;
            }

            public OctreeNode(T data, int rank)
            {
                Data = data;
                _rank = rank;
                _size = 1 << rank;
            }
        }

        private readonly OctreeNode _defaultNode;

        public OctreeNode DefaultNode
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _defaultNode;
        }

        public Octree(T defaultData, int maxRank)
        {
            _defaultNode = new OctreeNode(defaultData, maxRank);
        }
    }
}