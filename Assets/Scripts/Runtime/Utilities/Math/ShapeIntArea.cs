using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Utilities.Math
{
    public struct ShapeIntArea<T> : IAreaInt
        where T : unmanaged, IShapeInt
    {
        public T Shape { get; }
        public IShapeInt AbstractShape { get => Shape; }
        public Vector3Int Anchor { get; }

        public ShapeIntArea(T shape, Vector3Int anchor) : this()
        {
            Shape = shape;
            Anchor = anchor;
        }

        public ShapeIntArea<DisposableArbitraryShapeInt> BooleanCutViaOtherArea<TMerging>(
            ShapeIntArea<TMerging> otherArea)
            where TMerging: unmanaged, IShapeInt
        {
            int totalVolume = AbstractShape.Volume + otherArea.AbstractShape.Volume;
            NativeParallelHashMap<Vector3Int, int> pointToIndexAssociations
                = new(totalVolume, Allocator.Persistent);

            NativeParallelHashMap<int, Vector3Int> indexToPointAssociations
                = new(totalVolume, Allocator.Persistent);

            var job = new BooleanCutShapeIntAreasJob<T, TMerging>(
                pointToIndexAssociations, indexToPointAssociations, this, otherArea);

            var handler = job.Schedule(totalVolume, 64);

            handler.Complete();

            return new ShapeIntArea<DisposableArbitraryShapeInt>(
                new DisposableArbitraryShapeInt(pointToIndexAssociations, indexToPointAssociations),
                Anchor);
        }
    }
}