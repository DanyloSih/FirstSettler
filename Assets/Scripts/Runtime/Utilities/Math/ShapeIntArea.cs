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
    }
}