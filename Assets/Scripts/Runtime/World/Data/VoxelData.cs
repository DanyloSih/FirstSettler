namespace World.Data
{
#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    public struct VoxelData
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
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
