using System;
using UnityEngine;

namespace MarchingCubesProject
{
    [Serializable]
    public struct ChunkGenerationSettings
    {
        [SerializeField] public float MaxHeight;
        [SerializeField] public float MinHeight;
        [SerializeField] public PerlinNoiseSettings PerlinNoiseSettings;
        [SerializeField] public int Seed;
        [SerializeField] public Vector3Int VoxelsOffset;
    }
}
