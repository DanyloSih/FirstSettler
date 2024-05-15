using UnityEngine;

namespace SimpleChunks
{
    public static class IPositionalHashMapExtensions
    {
        /// <summary>
        /// <inheritdoc cref="IPositionalHashMap{TValue}.TryGetValue(int, int, int, out TValue)"/>
        /// </summary>
        public static bool TryGetValue<TValue>(this IPositionalHashMap<TValue> valueContainer, Vector3Int position, out TValue value)
        {
            return valueContainer.TryGetValue(position.x, position.y, position.z, out value);
        }

        /// <summary>
        /// <inheritdoc cref="IPositionalHashMap{TValue}.AddValue(int, int, int, TValue)"/>
        /// </summary>
        public static void AddValue<TValue>(this IPositionalHashMap<TValue> valueContainer, Vector3Int position, TValue value)
        {
            valueContainer.AddValue(position.x, position.y, position.z, value);
        }

        /// <summary>
        /// <inheritdoc cref="IPositionalHashMap{TValue}.RemoveValue(int, int, int)"/>
        /// </summary>
        public static void RemoveValue<TValue>(this IPositionalHashMap<TValue> valueContainer, Vector3Int position)
        {
            valueContainer.RemoveValue(position.x, position.y, position.z);
        }

        /// <summary>
        /// <inheritdoc cref="IPositionalHashMap{TValue}.IsValueExist(int, int, int)"/>
        /// </summary>
        public static bool IsValueExist<TValue>(this IPositionalHashMap<TValue> valueContainer, Vector3Int position)
        {
            return valueContainer.IsValueExist(position.x, position.y, position.z);
        }
    }
}
