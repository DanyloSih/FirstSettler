namespace World.Data
{
    public struct VoxelData
    {
        public float Volume;
        public int MaterialHash;

        public static bool operator ==(VoxelData left, VoxelData right)
        {
            return left.Volume == right.Volume
                && left.MaterialHash == right.MaterialHash;
        }

        public static bool operator !=(VoxelData left, VoxelData right)
        {
            return !(left == right);
        }
    }
}
