using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Utilities.Math
{
    public struct DisposableArbitraryShapeInt : IShapeInt, IDisposable
    {
        private NativeHashMap<Vector3Int, int> _pointToIndexAssociations;
        private NativeHashMap<int, Vector3Int> _indexToPointAssociations;

        public int Volume { get; }

        public DisposableArbitraryShapeInt(
           IEnumerable<Vector3Int> points, Allocator allocator) : this()
        {
            _pointToIndexAssociations = new NativeHashMap<Vector3Int, int>(20, allocator);
            _indexToPointAssociations = new NativeHashMap<int, Vector3Int>(20, allocator);

            int counter = 0;
            foreach (var point in points)
            {
                _pointToIndexAssociations.Add(point, counter);
                _indexToPointAssociations.Add(counter, point);
                counter++;
            }

            Volume = counter;
        }

        public DisposableArbitraryShapeInt(
            NativeHashMap<Vector3Int, int> pointToIndexAssociations,
            NativeHashMap<int, Vector3Int> indexToPointAssociations) : this()
        {
            if (pointToIndexAssociations.Count != indexToPointAssociations.Count)
            {
                throw new ArgumentException($"Hash maps must have the same number of elements!");
            }

            Volume = pointToIndexAssociations.Count;

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