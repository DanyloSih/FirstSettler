using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
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

    public struct Octree<T> : IDisposable
        where T : unmanaged
    {
        public const int POSITION_LIMIT = 511;
        public const int HALF_POSITION_LIMIT = POSITION_LIMIT / 2;

        private readonly int _maxRank;
        private readonly OctreeNode<T> _rootNode;
        private readonly int _size;
        private readonly int _volume;
        private NativeParallelHashMap<int, OctreeNode<T>> _dataMap;
        private NativeParallelHashMap<int, bool> _equalityMap;

        /// <summary>
        /// Determines how many "elementary subnodes" (node with rank 0) can fit inside this node.
        /// </summary>
        public int Size
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _size;
        }
        /// <summary>
        /// <inheritdoc cref="OctreeNode{T}.Rank"/>
        /// </summary>
        public int MaxRank
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _maxRank;
        }
        public OctreeNode<T> RootNode
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _rootNode;
        }

        public Octree(T defaultData, OctreeRank maxRank)
        {
            _maxRank = (int)maxRank;
            _rootNode = new OctreeNode<T>(defaultData, _maxRank, Vector3Int.zero);
            _size = 1 << _maxRank;
            _volume = _size * _size * _size;
            _dataMap = new NativeParallelHashMap<int, OctreeNode<T>>(_volume, Allocator.Persistent);
            _dataMap.Add(GetNodeHash(_rootNode), _rootNode);
            _equalityMap = new NativeParallelHashMap<int, bool>(_volume, Allocator.Persistent);
        }

        public void Dispose()
        {
            _dataMap.Dispose();
            _equalityMap.Dispose();
        }

        /// <param name="rank">Rank of overriding node, min 0 (inclusive), 
        /// max depends on <see cref="MaxRank"/> of this octree (inclusive). <br/> 
        /// For example, if rank = 5, then all nodes with ranks below 5 located in the specified region will be deleted.</param>
        /// <param name="position">Position inside overriding node.</param>
        /// <param name="data">Data of overriding node.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetData(T data, int rank, Vector3Int position)
        {
            Vector3Int roundedPosition = RoundVectorToRank(rank, position);
            int hash = GetNodeHash(rank, roundedPosition);
            int nodeParentRank = rank + 1;
            if (nodeParentRank <= _maxRank)
            {

            }

            SetNode(hash, data, rank, position);

            for (int i = MaxRank; i >= rank; i--)
            {
                RoundVectorToRank(i, roundedPosition);
                
                
                
            }
        }

        //private T GetFillData(int rank, Vector3Int position)
        //{
        //    for (int i = rank + 1; i <= _maxRank; i++)
        //    {
        //        Vector3Int roundedPosition = RoundVectorToRank(i, position);
        //        int hash = GetNodeHash(i - 1, roundedPosition);
        //        bool isEqual = true;
        //        if (_equalityMap.ContainsKey(hash))
        //        {
        //            isEqual = _equalityMap[hash];
        //        }
        //        else
        //        {
        //            _equalityMap.Add(hash, isEqual);
        //        }


        //    }
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetNode(int hash, T data, int rank, Vector3Int position)
        {
            if (_dataMap.ContainsKey(hash))
            {
                OctreeNode<T> node = _dataMap[hash];
                node.Data = data;
                _dataMap[hash] = node;
            }
            else
            {
                _dataMap.Add(hash, new OctreeNode<T>(data, rank, position));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetNodeHash(int rank, Vector3Int nodePosition)
        {
            int rankFormated = rank & 15 << 28;
            int xFormated = (nodePosition.x + POSITION_LIMIT) & POSITION_LIMIT << 19;
            int yFormated = (nodePosition.y + POSITION_LIMIT) & POSITION_LIMIT << 10;
            int zFormated = (nodePosition.z + POSITION_LIMIT) & POSITION_LIMIT;
            return rankFormated | xFormated | yFormated | zFormated;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetNodeHash(OctreeNode<T> node)
        {
            return GetNodeHash(node.Rank, node.Position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3Int RoundVectorToRank(int rank, Vector3Int roundingVector)
        {
            ShiftVectorMembersRight(rank, ref roundingVector);
            ShiftVectorMembersLeft(rank, ref roundingVector);
            return roundingVector;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ShiftVectorMembersRight(int shiftingRank, ref Vector3Int shiftingVector)
        {
            shiftingVector.x = shiftingVector.x >> shiftingRank;
            shiftingVector.y = shiftingVector.y >> shiftingRank;
            shiftingVector.z = shiftingVector.z >> shiftingRank;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ShiftVectorMembersLeft(int shiftingRank, ref Vector3Int shiftingVector)
        {
            shiftingVector.x = shiftingVector.x << shiftingRank;
            shiftingVector.y = shiftingVector.y << shiftingRank;
            shiftingVector.z = shiftingVector.z << shiftingRank;
        }
    }
}