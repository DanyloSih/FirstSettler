namespace World.Organization
{
    public interface IChunksContainer
    {
        /// <summary>
        /// The maximum x or y or z coordinate value that this collection supports.
        /// </summary>
        public int MaxCoordinateValue { get; }
        /// <summary>
        /// The minimum x or y or z coordinate value that this collection supports.
        /// </summary>
        public int MinCoordinate { get; }

        public void ClearAllRecordsAboutChunks();

        /// <summary>
        /// CAN RETURN NULL!<br/>
        /// Returns chunk by its global coordinates in chunks list.
        /// </summary>
        public IChunk GetChunk(int x, int y, int z);

        /// <summary>
        /// THIS MEHTOD DON'T CHECK IS OTHER CHUNK AT THIS POSITION ALREADY EXIST!<br/>
        /// Be careful not to add multiple records about chunk on the same place.
        /// </summary>
        public void AddChunk(int x, int y, int z, IChunk chunk);

        /// <summary>
        /// THIS METHOD DON'T CHECK IS OTHER CHUNK AT THIS POSITION ALREADY EXIST!<br/>
        /// Be careful not to delete a chunk entry that doesn't exist.
        /// </summary>
        public void RemoveChunk(int x, int y, int z);

        /// <summary>
        /// Checks whether there is a record about some chunk at this position.
        /// </summary>
        public bool IsChunkExist(int x, int y, int z);
    }
}