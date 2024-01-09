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
    }
}