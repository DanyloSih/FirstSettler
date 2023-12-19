namespace World.Data
{
    public struct VoxelData
    {
        public float Volume;
        public int MaterialHash;

        public VoxelData(float volume, int materialHash)
        {
            Volume = volume;
            MaterialHash = materialHash;
        }
    }
}
