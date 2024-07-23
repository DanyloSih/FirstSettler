using UnityEngine;
using System.Collections.Generic;

namespace Utilities.Math
{
    public static class Neighbors
    {
        /// <summary>
        /// Contains offset vectors for all possible neighbors in 3D space, 
        /// including diagonal neighbors and the center. <br/>
        /// There are 26 neighbors + center = (27 offsets) in total. <br/>
        /// The order of neighbors is defined by <see cref="RectPrismInt"/> <br/>
        /// Neighbor offset vector example: (-1, 0, 1)
        /// </summary>
        private static List<Vector3Int> s_neighborOffsetsIn3D;
        private static RectPrismInt s_neighborsIn3DRect;
        private static Vector3Int s_neighbors3DOffset;

        static Neighbors()
        {
            s_neighborOffsetsIn3D = new List<Vector3Int>();
            s_neighborsIn3DRect = new RectPrismInt(new Vector3Int(3, 3, 3));
            s_neighbors3DOffset = -Vector3Int.one;

            foreach (var neighbor in IterateNeighborOffsetsIn3D())
            {
                s_neighborOffsetsIn3D.Add(neighbor);
            }
        }

        /// <summary>
        /// <inheritdoc cref="s_neighborOffsetsIn3D"/>
        /// </summary>
        public static IReadOnlyList<Vector3Int> NeighborsOffsetsIn3D => s_neighborOffsetsIn3D;
        public static RectPrismInt NeighborsIn3DRect => s_neighborsIn3DRect;

        /// <summary>
        /// Iterate <see cref="NeighborsOffsetsIn3D"/> list
        /// </summary>
        public static IEnumerable<Vector3Int> IterateNeighborOffsetsIn3D()
        {
            foreach (var point in s_neighborsIn3DRect.GetEveryPoint())
            {
                Vector3Int neighbor = point + s_neighbors3DOffset;
                yield return neighbor;
            }
        }
    }
}
