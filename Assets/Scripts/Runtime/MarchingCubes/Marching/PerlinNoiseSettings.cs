using System;
using UnityEngine;

namespace MarchingCubesProject
{
    [Serializable]
    public struct PerlinNoiseSettings
    {
        [SerializeField] public int Octaves;
        [SerializeField] public float Persistence;
        [SerializeField] public float Frequency;
        [SerializeField] public float Amplitude;
    }
}
