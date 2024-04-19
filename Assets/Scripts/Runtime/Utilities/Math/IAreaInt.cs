using UnityEngine;

namespace Utilities.Math
{
    public interface IAreaInt
    {
        public IShapeInt AbstractShape { get; }

        /// <summary>
        /// The left lower back point relative to which the <see cref="AbstractShape"/> coordinates are calculated.
        /// </summary>
        public Vector3Int Anchor { get; }
    }
}