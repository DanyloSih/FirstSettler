using UnityEngine;

namespace Utilities.Math
{
    public struct Vector3IntWithIndex
    {
        public Vector3Int Vector;
        public int Index;

        public Vector3IntWithIndex(Vector3Int vector, int index)
        {
            Vector = vector;
            Index = index;
        }
    }
}