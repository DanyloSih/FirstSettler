using UnityEngine;

namespace World.Data
{
    [CreateAssetMenu(
        fileName = nameof(GenerationAlgorithmInfo),
        menuName = "FirstSettler/World/Data/" + nameof(GenerationAlgorithmInfo))]
    public class GenerationAlgorithmInfo : ScriptableObject
    {
        [SerializeField] private int _maxTrianglesPerMarch;
        [SerializeField] private int _maxVerticesPerMarch;

        public int MaxTrianglesPerMarch { get => _maxTrianglesPerMarch; }
        public int MaxVerticesPerMarch { get => _maxVerticesPerMarch; }
    }
}
