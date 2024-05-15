using System.Runtime.CompilerServices;

namespace SimpleChunks
{
    public class ChunkNeighbors
    {
        private const int _WIDTH = 3;
        private const int _WIDTH_AND_HEIGHT = _WIDTH * _WIDTH;
        private const int _NEIGHBORS_COUNT = _WIDTH * _WIDTH * _WIDTH;

        public IChunk[] Neighbors { get; private set; }

        public ChunkNeighbors(
            int globalChunkXPos,
            int globalChunkYPos,
            int globalChunkZPos,
            ChunksContainer chunksContainer)
        {
            int x = globalChunkXPos, y = globalChunkYPos, z = globalChunkZPos;

            Neighbors = new IChunk[_NEIGHBORS_COUNT];

            for (int a = 0; a < _WIDTH; a++)
            {
                for (int b = 0; b < _WIDTH; b++)
                {
                    for (int c = 0; c < _WIDTH; c++)
                    {
                        chunksContainer.TryGetValue(x + a - 1, y + b - 1, z + c - 1, out var chunk);
                        Neighbors[GetIndex(a, b, c)] = chunk;
                    }
                }
            }
        }



        /// <summary>
        /// Using examples:<br/>
        /// To choose LeftBottomBackward neighbor you can use this arguments: -1, -1, -1;<br/>
        /// To choose RightTopForward neighbor you can use this arguments: 1, 1, 1;<br/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IChunk GetNeighbor(int xOffset, int yOffset, int zOffset)
        {
            return Neighbors[GetIndex(xOffset, yOffset, zOffset)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex(int x, int y, int z)
        {
            return x + y * _WIDTH + z * _WIDTH_AND_HEIGHT;
        }
    }
}