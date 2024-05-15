namespace SimpleChunks
{
    /// <summary>
    /// All keys in this hash map are unique hashes of unique positions.
    /// </summary>
    public interface IPositionalHashMap<TValue>
    {
        /// <summary>
        /// The maximum x or y or z coordinate value that this collection supports.
        /// </summary>
        public int MaxCoordinateValue { get; }
        /// <summary>
        /// The minimum x or y or z coordinate value that this collection supports.
        /// </summary>
        public int MinCoordinate { get; }

        public void ClearAllRecords();

        /// <summary>
        /// Returns value by its position.
        /// </summary>
        public bool TryGetValue(int x, int y, int z, out TValue value);

        /// <summary>
        /// Returns value by its position hash.
        /// </summary>
        public bool TryGetValue(long positionHash, out TValue value);

        /// <summary>
        /// THIS MEHTOD DON'T CHECK IS OTHER VALUE AT THIS POSITION ALREADY EXIST!<br/>
        /// Be careful not to add multiple records about chunk on the same place.
        /// </summary>
        public void AddValue(int x, int y, int z, TValue value);

        /// <summary>
        /// THIS METHOD DON'T CHECK IS OTHER VALUE AT THIS POSITION ALREADY EXIST!<br/>
        /// Be careful not to delete a value that doesn't exist.
        /// </summary>
        public void RemoveValue(int x, int y, int z);

        /// <summary>
        /// Checks whether there is a record about some value at this position.
        /// </summary>
        public bool IsValueExist(int x, int y, int z);
    }
}