using UnityEngine;

namespace Utilities.Math
{
    public interface IShapeInt
    {
        public int Volume { get; }

        Vector3Int IndexToPoint(int index);
        bool IsContainsPoint(Vector3Int point);
        int PointToIndex(Vector3Int point);
    }
}