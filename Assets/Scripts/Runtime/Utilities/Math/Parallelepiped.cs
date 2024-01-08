using UnityEngine;

namespace Utilities.Math
{
    public struct Parallelepiped
    {
        public Vector3Int Size;

        public Parallelepiped(Vector3Int size)
        {
            Size = size;
        }

        public float CalculateAreaToVolumeRatio()
        {
            float volume = CalculateVolume();
            float area = CalculateSurfaceArea();
            return area / volume;
        }

        public float CalculateVolume()
        {
            return Size.x * Size.y * Size.z;
        }

        public float CalculateSurfaceArea()
        {
            return 2 * (Size.x * Size.y + Size.x * Size.z + Size.y * Size.z);
        }
    }
}