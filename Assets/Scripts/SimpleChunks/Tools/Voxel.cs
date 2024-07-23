using System.Runtime.InteropServices;

namespace SimpleChunks.Tools
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Voxel
    {
        public const int TYPE_SIZE = (sizeof(int) + sizeof(long));

        [FieldOffset(0)]
        public int VoxelID;
        [FieldOffset(sizeof(int))]
        public long ChunkHash;

        public Voxel(int voxelID, long chunkHash)
        {
            VoxelID = voxelID;
            ChunkHash = chunkHash;
        }
    }
}
