using UnityEngine;

namespace SimpleChunks.DataGeneration
{
    [CreateAssetMenu(
        fileName = nameof(BasicChunkSettings),
        menuName = "FirstSettler/MarchingCubes/" + nameof(BasicChunkSettings))]
    public class BasicChunkSettings : ScriptableObject
    {
        [SerializeField] private Vector3Int _chunkSizeInCubes = new Vector3Int(16, 16, 16);
        [SerializeField] private float _scale = 1;

        public float Scale { get => _scale; }
        public Vector3Int SizeInCubes => _chunkSizeInCubes;
        public Vector3Int SizeInVoxels => _chunkSizeInCubes + Vector3Int.one;
        public Vector3Int ScaledSizeInCubes => new Vector3Int(
            (int)(_chunkSizeInCubes.x * _scale),
            (int)(_chunkSizeInCubes.y * _scale),
            (int)(_chunkSizeInCubes.z * _scale));
    }
}
