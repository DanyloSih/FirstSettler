using UnityEngine;

namespace World.Data
{

    [CreateAssetMenu(
        fileName = nameof(BasicChunkSettings),
        menuName = "FirstSettler/MarchingCubes/" + nameof(BasicChunkSettings))]
    public class BasicChunkSettings : ScriptableObject
    {
        [SerializeField] private int _width;
        [SerializeField] private int _height;
        [SerializeField] private int _depth;
        [SerializeField] private float _scale = 1;

        public int Width { get => _width; }
        public int Height { get => _height; }
        public int Depth { get => _depth; }
        public float Scale { get => _scale; }
        public Vector3Int Size => new Vector3Int(_width, _height, _depth);
        public Vector3Int SizePlusOne => new Vector3Int(_width + 1, _height + 1, _depth + 1);
        public Vector3Int ScaledSize => new Vector3Int(
            (int)(_width * _scale),
            (int)(_height * _scale),
            (int)(_depth * _scale));
    }
}
