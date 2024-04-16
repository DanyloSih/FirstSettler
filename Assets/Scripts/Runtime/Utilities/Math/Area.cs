using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utilities.Math
{
    public struct Area
    {
        public Parallelepiped Parallelepiped { get; private set; }

        public Vector3Int Center { get; private set; }
        public Vector3Int Min { get; private set; }
        public Vector3Int Max { get; private set; }

        public Area(Parallelepiped parallelepiped, Vector3Int centerPosition)
        {
            Parallelepiped = parallelepiped;
            Center = centerPosition;
            Min = Center - parallelepiped.Extents;
            Max = Center + parallelepiped.Extents;
        }

        public Area(Vector3Int min, Vector3Int max)
        {
            Vector3Int areaSize = max - min;
            Parallelepiped = new Parallelepiped(areaSize);
            Center = min + areaSize / 2;
            Min = min;
            Max = max;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<Vector3Int> GetEveryVoxel()
        {
            for (int i = 0; i < Parallelepiped.Volume; i++)
            {
                yield return Min + Parallelepiped.IndexToPoint(i);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int VoxelPositionToIndex(Vector3Int point)
        {
            return Parallelepiped.PointToIndex(point - Min);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3Int IndexToVoxelPosition(int index)
        {
            return Parallelepiped.IndexToPoint(index) + Min;
        }
    }
}