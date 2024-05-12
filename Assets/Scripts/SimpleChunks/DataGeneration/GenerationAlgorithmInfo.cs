using UnityEngine;

namespace SimpleChunks.DataGeneration
{
    [CreateAssetMenu(
        fileName = nameof(GenerationAlgorithmInfo),
        menuName = "FirstSettler/World/Data/" + nameof(GenerationAlgorithmInfo))]
    public class GenerationAlgorithmInfo : ScriptableObject
    {
        [SerializeField] private int _maxTrianglesPerMarch = 15;
        [SerializeField] private int _maxVerticesPerMarch = 15;
        [SerializeField] private float _surfaceFactor = 0.5f;

        public int MaxTrianglesPerMarch { get => _maxTrianglesPerMarch; }
        public int MaxVerticesPerMarch { get => _maxVerticesPerMarch; }
        public float SurfaceFactor { get => _surfaceFactor; }
    }
}
