using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Utilities.Math
{
    public static class SphereCash
    {
        private static Dictionary<int, NativeParallelHashMap<int, Vector3Int>> s_indexToPointAssociationsContainer;
        private static Dictionary<int, NativeParallelHashMap<Vector3Int, int>> s_pointToIndexAssociationsContainer;

        public static Dictionary<int, NativeParallelHashMap<int, Vector3Int>> IndexToPointAssociationsContainer 
            => s_indexToPointAssociationsContainer;
        public static Dictionary<int, NativeParallelHashMap<Vector3Int, int>> PointToIndexAssociationsContainer 
            => s_pointToIndexAssociationsContainer;

        static SphereCash()
        {
            s_indexToPointAssociationsContainer = new Dictionary<int, NativeParallelHashMap<int, Vector3Int>>(20);
            s_pointToIndexAssociationsContainer = new Dictionary<int, NativeParallelHashMap<Vector3Int, int>>(20);
            Application.quitting += OnApplicationQuit;
        }

        private static void OnApplicationQuit()
        {
            DisposeAssociationContainers();
        }

        private static void DisposeAssociationContainers()
        {
            foreach (var association in s_indexToPointAssociationsContainer)
            {
                association.Value.Dispose();
            }

            foreach (var association in s_pointToIndexAssociationsContainer)
            {
                association.Value.Dispose();
            }

            s_indexToPointAssociationsContainer.Clear();
            s_pointToIndexAssociationsContainer.Clear();
        }
    }
}