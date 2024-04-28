using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;

namespace Utilities.Math
{
    public struct SphereInt : IShapeInt
    {
        private readonly int _radius;
        private readonly int _diameter;
        private readonly int _volume;
        [ReadOnly]
        private readonly NativeHashMap<int, Vector3Int> _indexToPointAssociations;
        [ReadOnly]
        private readonly NativeHashMap<Vector3Int, int> _pointToIndexAssociations;

        public static int MaxRadius => PositionHasher.Y_MAX;
        public int Radius => _radius;
        public int Diameter => _diameter;
        public int Volume => _volume;

        public SphereInt(int radius)
        {
            _radius = radius;
            _diameter = radius * 2;

            if (!SphereCash.IndexToPointAssociationsContainer.ContainsKey(_radius))
            {
                var associations = CreateAssociations(_radius, new SpiralMatrixWalker());

                SphereCash.IndexToPointAssociationsContainer.Add(_radius, associations.Item1);
                SphereCash.PointToIndexAssociationsContainer.Add(_radius, associations.Item2);
            }

            _indexToPointAssociations = SphereCash.IndexToPointAssociationsContainer[_radius];
            _pointToIndexAssociations = SphereCash.PointToIndexAssociationsContainer[_radius];

            _volume = _indexToPointAssociations.Count;
        }  

        private static (NativeHashMap<int, Vector3Int>, NativeHashMap<Vector3Int, int>) CreateAssociations(
            int radius, IMatrixWalker matrixWalker)
        {
            var volume = GetVolume(radius);
            Vector3Int boxSize = Vector3Int.one * radius * 2;
            Vector3Int boxHalfSize = Vector3Int.one * radius;
            
            var result = (new NativeHashMap<int, Vector3Int>(volume, Allocator.Persistent), 
                new NativeHashMap<Vector3Int, int>(volume, Allocator.Persistent));

            int counter = 0;

            foreach (Vector3Int pos in matrixWalker.WalkMatrix(boxSize))
            {
                if (Vector3Int.Distance(boxHalfSize, pos) <= radius)
                {
                    result.Item1.Add(counter, pos);
                    result.Item2.Add(pos, counter);
                    counter++;
                }
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetVolume(int radius)
        {
            return (int)((4f / 3f) * Mathf.PI * (radius * radius * radius));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsContainsPoint(Vector3Int point)
        {
            return _pointToIndexAssociations.ContainsKey(point);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int PointToIndex(Vector3Int position)
        {
            return _pointToIndexAssociations[position];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3Int IndexToPoint(int index)
        {
            return _indexToPointAssociations[index];
        } 
    }
}