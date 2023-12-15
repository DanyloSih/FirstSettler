using UnityEngine;

namespace MarchingCubesProject
{
    [CreateAssetMenu(
        fileName = nameof(BasicChunkSettings),
        menuName = "FirstSettler/MarchingCubes/" + nameof(BasicChunkSettings))]
    public class BasicChunkSettings : ScriptableObject
    {
        [SerializeField] private int _width;
        [SerializeField] private int _height;
        [SerializeField] private int _depth;

        public int Width { get => _width; }
        public int Height { get => _height; }
        public int Depth { get => _depth; }

        public Vector3Int ChunkSize => new Vector3Int(_width, _height, _depth);
    }
}
