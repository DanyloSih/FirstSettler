using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace Utilities.Math
{
    public struct NativeOctree<T> : IDisposable
        where T : unmanaged
    {
        public const int POSITION_LIMIT = 511;
        public const int HALF_POSITION_LIMIT = POSITION_LIMIT / 2;
        private readonly int _maxRank;
        private readonly OctreeNode<T> _rootNode;
        private readonly int _size;
        private readonly int _volume;
        private bool _isInitialized;
        private NativeHashMap<int, OctreeNode<T>> _dataMap;
        private NativeHashMap<int, SubnodesContainer> _subnodesMap;

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
        public bool IsInitialized 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _isInitialized; 
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeOctree(T defaultData, OctreeRank maxRank)
        {
            _isInitialized = true;
            _maxRank = (int)maxRank;
            _rootNode = new OctreeNode<T>(defaultData, _maxRank, Vector3Int.zero);
            _size = 1 << _maxRank;
            _volume = _size * _size * _size;
            _dataMap = new NativeHashMap<int, OctreeNode<T>>(_volume, Allocator.Persistent);
            _subnodesMap = new NativeHashMap<int, SubnodesContainer>(_volume, Allocator.Persistent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _isInitialized = false;
            _dataMap.Dispose();
            _subnodesMap.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<OctreeNode<T>> GetNodes(Allocator allocator)
        {
            return _dataMap.GetValueArray(allocator);
        }

        /// <param name="rank">Rank of overriding node, min 0 (inclusive), 
        /// max depends on <see cref="MaxRank"/> of this octree (inclusive). <br/> 
        /// For example, if rank = 5, then all nodes with ranks below 5 located in the specified region will be deleted.</param>
        /// <param name="position">Position inside overriding node.</param>
        /// <param name="data">Data of overriding node.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetData(T data, int rank, Vector3Int position)
        {
            if (rank > _maxRank)
            {
                return;
            }
            position = RoundVectorToRank(rank, position);
            int parentRank = rank + 1;
            OctreeNode<T> newNode = new OctreeNode<T>(data, rank, position);
            int newNodeHash = GetNodeHash(newNode);

            OctreeNode<T> rootNode = FindRootNode(rank + 1, position, out int rootNodeHash);

            if (!_dataMap.ContainsKey(newNodeHash))
            {
                if (rank != _maxRank)
                {                  
                    if (_dataMap.ContainsKey(rootNodeHash) && rootNode.Data.GetHashCode() == data.GetHashCode())
                    {
                        return;
                    }
                    SplitNodesFromRootToChild(rootNode, newNode);
                }

                AddNode(newNode, newNodeHash);
            }
            else
            {
                OctreeNode<T> previousNode = _dataMap[newNodeHash];
                if (previousNode.Data.GetHashCode() == newNode.Data.GetHashCode())
                {
                    return;
                }
                _dataMap[newNodeHash] = newNode;
            }

            if (_subnodesMap.ContainsKey(newNodeHash))
            {
                RemoveSubnodesRecursively(newNodeHash);
            }

            CollapseIdenticalData(rootNode.Data, parentRank, position); 
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task SetDataFromArray(
            ThreedimensionalNativeArray<T> array, 
            Vector3Int arrayOffset = new Vector3Int(), 
            Vector3Int? size = null)
        {
            Vector3Int realSize = size == null ? array.Size : (Vector3Int)size;
            realSize += arrayOffset;

            for (int x = arrayOffset.x; x < realSize.x; x++)
            {
                for (int y = arrayOffset.y; y < realSize.y; y++)
                {
                    for (int z = arrayOffset.z; z < realSize.z; z++)
                    {
                        T value = array.GetValue(x, y, z);
                        SetData(value, 0, new Vector3Int(x, y, z));
                        await Task.Delay(5);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveSubnodesRecursively(int subnodesKeeperHash)
        {
            SubnodesContainer subnodes = _subnodesMap[subnodesKeeperHash];
            unsafe
            {
                int* ptr = &subnodes.ArrayStart;
                for (int i = 0; i < SubnodesContainer.CAPACITY; i++)
                {
                    int nextHash = *(ptr + i);
                    if (_subnodesMap.ContainsKey(nextHash))
                    {
                        RemoveSubnodesRecursively(nextHash);
                    }
                }
            }

            RemoveSubnodes(ref subnodes);
            _subnodesMap[subnodesKeeperHash] = subnodes;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private SubnodesContainer GenerateSubnodes(int rank, Vector3Int position)
        //{
        //    Vector3Int roundedPosition = RoundVectorToRank(rank, position);
            

        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CollapseIdenticalData(T defaultData, int parentRank, Vector3Int position)
        {
            Vector3Int parentRoundedPosition = RoundVectorToRank(parentRank, position);
            int parentHash = GetNodeHash(parentRank, parentRoundedPosition);

            InitializeSubnodes(parentHash);
            SubnodesContainer container = _subnodesMap[parentHash];
            if (IsAllDataInContainerEqual(ref container))
            {
                T data = container.Length < 1 ? defaultData : _dataMap[container.UnsafeGetValueAt(0)].Data;
                RemoveSubnodes(ref container);
                _subnodesMap[parentHash] = container;
                AddNode(new OctreeNode<T>(data, parentRank, parentRoundedPosition), parentHash);
                if (parentRank >= _maxRank)
                {
                    if (_dataMap.ContainsKey(parentHash))
                    {
                        RemoveNode(parentHash, parentRank, position);
                    }

                    return;
                }
                CollapseIdenticalData(defaultData, parentRank + 1, position);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsAllDataInContainerEqual(ref SubnodesContainer container)
        {
            if (container.Length < 1)
            {
                return true;
            }
            else
            {
                if (container.Length != SubnodesContainer.CAPACITY)
                {
                    return false;
                }

                unsafe
                {
                    fixed(int* ptr = &container.ArrayStart)
                    {
                        int firstDataHash = _dataMap[*(ptr)].Data.GetHashCode();
                        for (int i = 1; i < container.Length; i++)
                        {
                            if (firstDataHash != _dataMap[*(ptr + i)].Data.GetHashCode())
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SplitNodesFromRootToChild(OctreeNode<T> rootNode, OctreeNode<T> childNode)
        {
            if (rootNode.Rank < childNode.Rank)
            {
                return;
            }

            T rootData = rootNode.Data;
            int rootHash = GetNodeHash(rootNode);
            if (_dataMap.ContainsKey(rootHash))
            {
                RemoveNode(rootHash, rootNode.Rank, rootNode.Position);
            }

            for (int rank = rootNode.Rank - 1; rank >= childNode.Rank; rank--)
            {
                Vector3Int pos = RoundVectorToRank(rank + 1, childNode.Position);
                int parentHash = GetNodeHash(rank + 1, pos);
                InitializeSubnodes(parentHash);
                SubnodesContainer subnodes = _subnodesMap[parentHash];
                if (subnodes.Length > 0)
                {
                    continue;
                }
                int size = 1 << rank;
                AddNode(rootData, rank, new Vector3Int(pos.x, pos.y, pos.z), ref subnodes);
                AddNode(rootData, rank, new Vector3Int(pos.x + size, pos.y, pos.z), ref subnodes);
                AddNode(rootData, rank, new Vector3Int(pos.x, pos.y + size, pos.z), ref subnodes);
                AddNode(rootData, rank, new Vector3Int(pos.x + size, pos.y + size, pos.z), ref subnodes);
                AddNode(rootData, rank, new Vector3Int(pos.x, pos.y, pos.z + size), ref subnodes);
                AddNode(rootData, rank, new Vector3Int(pos.x + size, pos.y, pos.z + size), ref subnodes);
                AddNode(rootData, rank, new Vector3Int(pos.x, pos.y + size, pos.z + size), ref subnodes);
                AddNode(rootData, rank, new Vector3Int(pos.x + size, pos.y + size, pos.z + size), ref subnodes);
                _subnodesMap[parentHash] = subnodes;

                RemoveNode(rank, childNode.Position);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveNode(int nodeHash, int rank, Vector3Int position)
        {
            _dataMap.Remove(nodeHash);
            int parentRank = rank + 1;
            int parentRankHash = GetNodeHash(parentRank, RoundVectorToRank(parentRank, position));
            InitializeSubnodes(parentRankHash);
            SubnodesContainer subnodes = _subnodesMap[parentRankHash];
            subnodes.RemoveByValue(nodeHash);
            _subnodesMap[parentRankHash] = subnodes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveNode(int rank, Vector3Int position)
        {
            int nodeHash = GetNodeHash(rank, RoundVectorToRank(rank, position));
            RemoveNode(nodeHash, rank, position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddNode(T data, int rank, Vector3Int roundedPosition, ref SubnodesContainer parentSubnodesContainer)
        {
            int newNodeHash = GetNodeHash(rank, roundedPosition);
            if (_dataMap.ContainsKey(newNodeHash))
            {
                throw new InvalidOperationException("PIZDEC!");
            }
            _dataMap.Add(newNodeHash, new OctreeNode<T>(data, rank, roundedPosition));
            parentSubnodesContainer.AddValue(newNodeHash);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddNode(OctreeNode<T> newNode, int newNodeHash)
        {
            if (_dataMap.ContainsKey(newNodeHash))
            {
                throw new InvalidOperationException("PIZDEC!");
            }

            int parentRank = newNode.Rank + 1;
            int parentHash = GetNodeHash(parentRank, RoundVectorToRank(parentRank, newNode.Position));
            _dataMap.Add(newNodeHash, newNode);
            InitializeSubnodes(parentHash);
            SubnodesContainer subnodes = _subnodesMap[parentHash];
            subnodes.AddValue(newNodeHash);
            _subnodesMap[parentHash] = subnodes;
        }

        private OctreeNode<T> FindRootNode(int rank, Vector3Int position, out int rootNodeHash)
        {
            if (rank >= _maxRank)
            {
                Vector3Int rootNodePosition = RoundVectorToRank(_maxRank, position);
                rootNodeHash = GetNodeHash(_maxRank, rootNodePosition);
                return new OctreeNode<T>(_rootNode.Data, _rootNode.Rank, rootNodePosition);
            }

            rootNodeHash = GetNodeHash(rank, RoundVectorToRank(rank, position));
            if (_dataMap.ContainsKey(rootNodeHash))
            {
                return _dataMap[rootNodeHash];
            }
            else
            {
                return FindRootNode(rank + 1, position, out rootNodeHash);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveSubnodes(ref SubnodesContainer container)
        {
            unsafe
            {
                fixed (int* ptr = &container.ArrayStart)
                { 
                    for (int i = 0; i < container.Length; i++)
                    {
                        _dataMap.Remove(*(ptr + i));
                    }
                }
            }
            container.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitializeSubnodes(int subnodesKeeperHash)
        {
            if (!_subnodesMap.ContainsKey(subnodesKeeperHash))
            {
                _subnodesMap.Add(subnodesKeeperHash, new SubnodesContainer());
            }
        }

#region Static
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetNodeHash(int rank, Vector3Int nodePosition)
        {
            int rankFormated = (rank & 15) << 28;
            int xFormated = ((nodePosition.x + HALF_POSITION_LIMIT) & POSITION_LIMIT) << 19;
            int yFormated = ((nodePosition.y + HALF_POSITION_LIMIT) & POSITION_LIMIT) << 10;
            int zFormated = (nodePosition.z + HALF_POSITION_LIMIT) & POSITION_LIMIT;
            return rankFormated | xFormated | yFormated | zFormated;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetNodeHash(OctreeNode<T> node)
        {
            return GetNodeHash(node.Rank, node.Position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int RoundVectorToRank(int rank, Vector3Int roundingVector)
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
#endregion
    }
}