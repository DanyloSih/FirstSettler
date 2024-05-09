namespace MeshGeneration
{
    public struct Vector4Int
    {
        public int x; 
        public int y; 
        public int z; 
        public int w;

        public override string ToString()
        {
            return $"({x}, {y}, {z}, {w})";
        }
    }
}
