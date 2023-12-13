using System.Collections.Generic;
using UnityEngine;

namespace MarchingCubesProject
{
    [CreateAssetMenu(
        fileName = nameof(MaterialKeysPack),
        menuName = "FirstSettler/MarchingCubes/" + nameof(MaterialKeysPack))]
    public class MaterialKeysPack : ScriptableObject
    {
        [SerializeField] private List<MaterialKey> _materialKeys;

        private Dictionary<int, MaterialKey> _materialsHashTable
            = new Dictionary<int, MaterialKey>();

        public IReadOnlyList<MaterialKey> MaterialKeys { get => _materialKeys; }

        private void OnEnable()
        {
            _materialsHashTable.Clear();
            foreach (var key in _materialKeys)
            {
                _materialsHashTable.Add(key.GetHashCode(), key);
            }
        }

        public IEnumerable<MaterialKey> GetMaterialKeys() 
            => _materialKeys;

        public IEnumerable<int> GetMaterialKeyHashes()
        {
            foreach (var item in _materialsHashTable)
            {
                yield return item.Key;
            }   
        }

        public IEnumerable<KeyValuePair<int, MaterialKey>> GetAssociations()
            => _materialsHashTable;

        public MaterialKey GetMaterialKeyByHash(int hash)
        {
            if (_materialsHashTable.TryGetValue(hash, out var value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }
    }
}
