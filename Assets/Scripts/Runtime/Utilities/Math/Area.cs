using UnityEngine;

namespace Utilities.Math
{
    public struct Area
    {
        public Parallelepiped Parallelepiped { get; private set; }
        public Vector3Int Extents { get; private set; }
        public Vector3Int Center { get; private set; }
        public Vector3Int Min { get; private set; }
        public Vector3Int Max { get; private set; }

        public Area(Parallelepiped parallelepiped, Vector3Int centerPosition)
        {
            Parallelepiped = parallelepiped;
            Extents = parallelepiped.Size / 2;
            Center = centerPosition;
            Min = Center - Extents;
            Max = Center + Extents;
        }
    }
}