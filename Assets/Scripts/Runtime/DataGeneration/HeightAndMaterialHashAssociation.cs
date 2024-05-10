namespace DataGeneration
{
    public struct HeightAndMaterialHashAssociation
    {
        public float Height;
        public int MaterialHash;

        public HeightAndMaterialHashAssociation(float height, int materialHash)
        {
            Height = height;
            MaterialHash = materialHash;
        }
    }
}
