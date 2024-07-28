using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace Utilities.Math
{
    public struct NativeOctree<T> : IDisposable
        where T : unmanaged, IEquatable<T>
    {
        private T _defaultData;
        private readonly byte _maxRank;
        private readonly int _size;
        private readonly RectPrismInt _nodesLayerRectPrism;
        private readonly RectPrismInt _octreeRectPrism;
        private readonly CancellationTokenSource _disposingCancellationSource;
        private bool _isInitialized;
        private NativeHashMap<OctreeNode<T>, T> _data;

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
        public byte MaxRank
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _maxRank;
        }
        public bool IsInitialized 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _isInitialized; 
        }

        public CancellationToken DisposingCancellationToken
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _disposingCancellationSource.Token;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeOctree(T defaultData, OctreeRank octreeRank)
        {
            _defaultData = defaultData;
            _maxRank = (byte)octreeRank;
            _isInitialized = true;
            _size = 1 << _maxRank;
            _nodesLayerRectPrism = new RectPrismInt(new Vector3Int(2, 2, 2));
            _octreeRectPrism = new RectPrismInt(Vector3Int.one * _size);
            _disposingCancellationSource = new CancellationTokenSource();
            _data = new NativeHashMap<OctreeNode<T>, T>(8, Allocator.Persistent) 
            {
                { new OctreeNode<T>(defaultData, _maxRank, Vector3Int.zero) , defaultData }
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _disposingCancellationSource.Cancel();
            _isInitialized = false;
            _data.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNodeByGlobalPosition(Vector3Int globalNodePosition, out OctreeNode<T> node)
        {
            if (!_octreeRectPrism.IsContainsPoint(globalNodePosition))
            {
                node = new OctreeNode<T>();
                return false;
            }

            var startNode = OctreeNode<T>.FromGlobalPosition(_defaultData, 0, globalNodePosition);

            if (_data.ContainsKey(startNode))
            {
                node = new OctreeNode<T>(_data[startNode], startNode.Rank, startNode.RankPosition);
                return true;
            }

            return TryFindParentNode(startNode, out node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<OctreeNode<T>> GetNodes(Allocator allocator)
        {
            return _data.GetKeyArray(allocator);
        }

        /// <param name="rank">Rank of overriding node, min 0 (inclusive), 
        /// max depends on <see cref="MaxRank"/> of this octree (inclusive). <br/> 
        /// For example, if rank = 5, then all nodes with ranks below 5 located in the specified region will be deleted.</param>
        /// <param name="position">Position inside overriding node.</param>
        /// <param name="data">Data of overriding node.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(OctreeNode<T> newNode)
        {
            if (!_octreeRectPrism.IsContainsPoint(newNode.GlobalPosition))
            {
                return;
            }

            RemoveAllSubnodesInsideNode(newNode);

            if (newNode.Rank >= _maxRank)
            {
                SetNode(new OctreeNode<T>(newNode.Data, _maxRank, Vector3Int.zero));
                return;
            }

            if (TryFindParentNode(newNode, out var parentNode))
            {
                if (!parentNode.Data.Equals(newNode.Data))
                {
                    _data.Remove(parentNode);
                    SplitNodes(newNode, parentNode);
                }
            }
            else
            {
                if (!TryMergeNodes(newNode))
                {
                    SetNode(newNode);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task SetDataFromArray(
            ThreedimensionalNativeArray<T> array, 
            Vector3Int arrayOffset = new Vector3Int(), 
            Vector3Int? size = null,
            int delayInMilliseconds = 0,
            CancellationToken? cancellationToken = null)
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
                        if (cancellationToken != null && cancellationToken.Value.IsCancellationRequested)
                        {
                            return;
                        }

                        Insert(new OctreeNode<T>(value, 0, new Vector3Int(x, y, z)));

                        if (delayInMilliseconds > 0)
                        {
                            await Task.Delay(delayInMilliseconds);
                        }
                    }
                }
            }
        }

        private void SetNode(OctreeNode<T> node)
        {
            if (_data.ContainsKey(node))
            {
                _data[node] = node.Data;
            }
            else
            {
                _data.Add(node, node.Data);
            }
        }

        private bool TryMergeNodes(OctreeNode<T> newNode)
        {
            if (newNode.Rank < 0 || newNode.Rank >= _maxRank)
            {
                return false;
            }

            // "Virtual" means it doesn't have to actually exist in _data.
            int virtualParentRank = newNode.Rank + 1;
            OctreeNode<T> virtualParent = OctreeNode<T>.FromGlobalPosition(newNode.Data, virtualParentRank, newNode.GlobalPosition);

            OctreeNode<T> firstSubnode = OctreeNode<T>.FromGlobalPosition(newNode.Data, newNode.Rank, virtualParent.GlobalPosition);
            NativeArray<OctreeNode<T>> subnodes = new NativeArray<OctreeNode<T>>(_nodesLayerRectPrism.Volume, Allocator.Temp);

            bool isMergable = true;
            for (int i = 0; i < _nodesLayerRectPrism.Volume; i++)
            {
                Vector3Int offset = _nodesLayerRectPrism.IndexToPoint(i);
                OctreeNode<T> currentSubnode = new OctreeNode<T>(firstSubnode.Data, firstSubnode.Rank, firstSubnode.RankPosition + offset);

                if (!currentSubnode.Equals(newNode))
                {
                    if (!_data.ContainsKey(currentSubnode) || !_data[currentSubnode].Equals(newNode.Data))
                    {
                        isMergable = false;
                        break;
                    }
                }

                subnodes[i] = currentSubnode;
            }

            if (isMergable)
            {
                for (int i = 0; i < subnodes.Length; i++)
                {
                    OctreeNode<T> subnode = subnodes[i];
                    _data.Remove(subnode);
                }
                subnodes.Dispose();
                _data.Add(virtualParent, virtualParent.Data);
                TryMergeNodes(virtualParent);
                return true;
            }
            else
            {
                subnodes.Dispose();
                return false;
            }   
        }

        /// <summary>
        /// Splits <paramref name="parentNode"/> into subnodes up to the .Rank of the <paramref name="newNode"/> and add subnodes to _data. <br/> 
        /// Does not remove parentNode from _data .
        /// </summary>
        private void SplitNodes(OctreeNode<T> newNode, OctreeNode<T> parentNode)
        {
            int currentRank = parentNode.Rank - 1;

            if(currentRank < newNode.Rank)
            {
                return;
            }

            OctreeNode<T> firstSubnode = OctreeNode<T>.FromGlobalPosition(parentNode.Data, currentRank, parentNode.GlobalPosition);

            for (int i = 0; i < _nodesLayerRectPrism.Volume; i++)
            {
                Vector3Int offset = _nodesLayerRectPrism.IndexToPoint(i);
                OctreeNode<T> currentSubnode = new OctreeNode<T>(firstSubnode.Data, firstSubnode.Rank, firstSubnode.RankPosition + offset);

                if(currentSubnode.Equals(OctreeNode<T>.FromGlobalPosition(newNode.Data, currentRank, newNode.GlobalPosition)))
                {
                    if (currentRank == newNode.Rank)
                    {
                        _data.Add(newNode, newNode.Data);
                    }
                    else
                    {
                        SplitNodes(newNode, currentSubnode);
                    }
                }
                else
                {
                    _data.Add(currentSubnode, currentSubnode.Data);
                }
            }
        }

        /// <summary>
        /// Recursively removes all possible nodes within the passed node.
        /// </summary>
        private void RemoveAllSubnodesInsideNode(OctreeNode<T> node)
        {
            int subrank = node.Rank - 1;
            if (subrank < 0)
            {
                return;
            }

            OctreeNode<T> firstSubnode = OctreeNode<T>.FromGlobalPosition(node.Data, (byte)subrank, node.GlobalPosition);       

            for (int i = 0; i < _nodesLayerRectPrism.Volume; i++)
            {
                Vector3Int offset = _nodesLayerRectPrism.IndexToPoint(i);
                OctreeNode<T> currentSubnode = new OctreeNode<T>(firstSubnode.Data, firstSubnode.Rank, firstSubnode.RankPosition + offset);
                if (_data.ContainsKey(currentSubnode))
                {
                    _data.Remove(currentSubnode);
                }
                else
                {
                    RemoveAllSubnodesInsideNode(currentSubnode);
                }
            }
        }

        /// <summary>
        /// Return first existing node that "contains" <paramref name="target"/>
        /// </summary>
        private bool TryFindParentNode(OctreeNode<T> target, out OctreeNode<T> parent)
        {
            parent = new OctreeNode<T>();

            if (target.Rank >= _maxRank)
            {
                return false;
            }

            T data = target.Data;
            Vector3Int globalPos = target.GlobalPosition;

            for (int i = target.Rank + 1; i <= _maxRank; i++)
            {
                OctreeNode<T> currentNode = OctreeNode<T>.FromGlobalPosition(data, i, globalPos);
                if (_data.ContainsKey(currentNode))
                {
                    currentNode.Data = _data[currentNode];
                    parent = currentNode;
                    return true;
                }
            }

            return false;
        }


#region Static
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