using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Utilities.Math
{
    public struct SphereInt
    {
        private static Dictionary<int, NativeHashMap<int, Vector3Int>> s_indexToPointAssociationsContainer;
        private static Dictionary<int, NativeHashMap<Vector3Int, int>> s_pointToIndexAssociationsContainer;

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

        static SphereInt()
        {
            s_indexToPointAssociationsContainer = new Dictionary<int, NativeHashMap<int, Vector3Int>>(20);
            s_pointToIndexAssociationsContainer = new Dictionary<int, NativeHashMap<Vector3Int, int>>(20);
            Application.quitting += OnApplicationQuit;
        }

        private static void OnApplicationQuit()
        {
            DisposeAssociationContainers();
        }

        private static void DisposeAssociationContainers()
        {
            foreach (var association in s_indexToPointAssociationsContainer)
            {
                association.Value.Dispose();
            }

            foreach (var association in s_pointToIndexAssociationsContainer)
            {
                association.Value.Dispose();
            }

            s_indexToPointAssociationsContainer.Clear();
            s_pointToIndexAssociationsContainer.Clear();
        }

        public SphereInt(int radius)
        {
            _radius = radius;
            _diameter = radius * 2;

            if (!s_indexToPointAssociationsContainer.ContainsKey(_radius))
            {
                var associations = CreateAssociations(_radius);

                s_indexToPointAssociationsContainer.Add(_radius, associations.Item1);
                s_pointToIndexAssociationsContainer.Add(_radius, associations.Item2);
            }

            _indexToPointAssociations = s_indexToPointAssociationsContainer[_radius];
            _pointToIndexAssociations = s_pointToIndexAssociationsContainer[_radius];

            _volume = _indexToPointAssociations.Capacity;
        }  

        private static (NativeHashMap<int, Vector3Int>, NativeHashMap<Vector3Int, int>) CreateAssociations(int radius)
        {
            var volume = GetVolume(radius);
            var prism = new RectPrismInt(Vector3Int.one * radius * 2);
            var result = (new NativeHashMap<int, Vector3Int>(volume, Allocator.Persistent), 
                new NativeHashMap<Vector3Int, int>(volume, Allocator.Persistent));

            int counter = 0;
            foreach (var pos in prism.GetEveryPoint())
            {
                if (Vector3Int.Distance(prism.Extents, pos) <= radius)
                {
                    result.Item1.Add(counter, pos);
                    result.Item2.Add(pos, counter);
                    counter++;
                }
            }
            return result;
        }

        private static int GetVolume(int radius)
        {
            
            return (int)((4f / 3f) * Mathf.PI * (radius * radius * radius));
        }

        public Vector3Int PointToIndex(int index)
        {
            return _indexToPointAssociations[index];
        }

        public int PointToIndex(Vector3Int position)
        {
            return _pointToIndexAssociations[position];
        }

        public IEnumerable<Vector3Int> GetEveryPoint()
        {
            foreach (var item in _indexToPointAssociations)
            {
                yield return item.Value;
            }
        }

        public IEnumerable<int> GetEveryIndex()
        {
            for (int i = 0; i < _volume; i++)
            {
                yield return i;
            }
        }
    }
}