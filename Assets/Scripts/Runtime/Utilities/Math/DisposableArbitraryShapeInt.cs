using System;
using Unity.Collections;
using UnityEngine;

namespace Utilities.Math
{
    public struct DisposableArbitraryShapeInt : IShapeInt, IDisposable
    {
        private NativeParallelHashMap<Vector3Int, int> _pointToIndexAssociations;
        private NativeParallelHashMap<int, Vector3Int> _indexToPointAssociations;

        public int Volume { get; }

        public DisposableArbitraryShapeInt(
            NativeParallelHashMap<Vector3Int, int> pointToIndexAssociations, 
            NativeParallelHashMap<int, Vector3Int> indexToPointAssociations) : this()
        {
            if (pointToIndexAssociations.Count() != indexToPointAssociations.Count())
            {
                throw new ArgumentException($"Hash maps must have the same number of elements!");
            }

            Volume = pointToIndexAssociations.Count();

            _pointToIndexAssociations = pointToIndexAssociations;
            _indexToPointAssociations = indexToPointAssociations;
        }

        public void Dispose()
        {
            _pointToIndexAssociations.Dispose();
            _indexToPointAssociations.Dispose();
        }

        public bool IsContainsPoint(Vector3Int point)
        {
            return _pointToIndexAssociations.ContainsKey(point);
        }

        public Vector3Int IndexToPoint(int index)
        {
            return _indexToPointAssociations[index];
        }

        public int PointToIndex(Vector3Int point)
        {
            return _pointToIndexAssociations[point];
        }
    }
}