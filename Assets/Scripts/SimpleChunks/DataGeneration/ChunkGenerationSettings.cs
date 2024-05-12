using System;
using UnityEngine;

namespace SimpleChunks.DataGeneration
{
    [Serializable]
    public struct ChunkGenerationSettings
    {
        [SerializeField] public float MaxHeight;
        [SerializeField] public float MinHeight;
        [SerializeField] public int Seed;
        [SerializeField] public Vector3Int VoxelsOffset;
    }
}
