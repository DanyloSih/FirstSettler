using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Utilities.Math.Extensions;

namespace Utilities.Math
{
    public struct BooleanCutShapeIntAreasJob<A, B> : IJobParallelFor
        where A : unmanaged, IShapeInt
        where B : unmanaged, IShapeInt
    {
        [WriteOnly]
        private NativeParallelHashMap<Vector3Int, int> _pointToIndexAssociations;
        [WriteOnly]
        private NativeParallelHashMap<int, Vector3Int> _indexToPointAssociations;

        [ReadOnly]
        private ShapeIntArea<A> _areaA;
        [ReadOnly]
        private ShapeIntArea<B> _areaB;

        public BooleanCutShapeIntAreasJob(
            NativeParallelHashMap<Vector3Int, int> pointToIndexAssociations,
            NativeParallelHashMap<int, Vector3Int> indexToPointAssociations,
            ShapeIntArea<A> areaA,
            ShapeIntArea<B> areaB)
        {
            _pointToIndexAssociations = pointToIndexAssociations;
            _indexToPointAssociations = indexToPointAssociations;
            _areaA = areaA;
            _areaB = areaB;
        }

        public void Execute(int index)
        {
            var targetAreaPoint = _areaA.IndexToPoint(index);

            if (!_areaB.IsContainsPoint(targetAreaPoint))
            {
                var count = _pointToIndexAssociations.Count();
                _pointToIndexAssociations.Add(targetAreaPoint, count);
                _indexToPointAssociations.Add(count, targetAreaPoint);
            }        
        }
    }
}