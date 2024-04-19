using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utilities.Math
{
    public struct RectPrismAreaInt
    {
        public RectPrismInt RectPrism { get; private set; }

        public Vector3Int Center { get; private set; }
        public Vector3Int Min { get; private set; }
        public Vector3Int Max { get; private set; }

        public RectPrismAreaInt(RectPrismInt parallelepiped, Vector3Int centerPosition)
        {
            RectPrism = parallelepiped;
            Center = centerPosition;
            Min = Center - parallelepiped.Extents;
            Max = Center + parallelepiped.Extents;
        }

        public RectPrismAreaInt(Vector3Int min, Vector3Int max)
        {
            Vector3Int areaSize = max - min;
            RectPrism = new RectPrismInt(areaSize);
            Center = min + areaSize / 2;
            Min = min;
            Max = max;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<Vector3Int> GetEveryVoxel()
        {
            for (int i = 0; i < RectPrism.Volume; i++)
            {
                yield return Min + RectPrism.IndexToPoint(i);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int VoxelPositionToIndex(Vector3Int point)
        {
            return RectPrism.PointToIndex(point - Min);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3Int IndexToVoxelPosition(int index)
        {
            return RectPrism.IndexToPoint(index) + Min;
        }
    }
}