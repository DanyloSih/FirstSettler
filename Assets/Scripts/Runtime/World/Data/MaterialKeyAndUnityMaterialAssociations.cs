using System;
using System.Collections.Generic;
using UnityEngine;

namespace World.Data
{
    [CreateAssetMenu(
        fileName = nameof(MaterialKeyAndUnityMaterialAssociations),
        menuName = "FirstSettler/MarchingCubes/" + nameof(MaterialKeyAndUnityMaterialAssociations))]
    public class MaterialKeyAndUnityMaterialAssociations : ScriptableObject
    {
        [SerializeField] private List<KeyAndMaterialAssociation> _keyAndMaterialAssociations;

        private Dictionary<int, Material> _keyHashAndMaterialAssociations = new Dictionary<int, Material>();

        protected void OnEnable()
        {
            _keyHashAndMaterialAssociations.Clear();
            foreach (var item in _keyAndMaterialAssociations)
            {
                _keyHashAndMaterialAssociations.Add(item.MaterialKey.GetHashCode(), item.UnityMaterial);
            }
        }

        public IEnumerable<int> GetMaterialKeyHashes()
        {
            foreach (var item in _keyHashAndMaterialAssociations)
            {
                yield return item.Key;    
            }
        }

        public IEnumerable<KeyValuePair<int, Material>> GetMaterialKeyHashAndMaterialAssociations()
            => _keyHashAndMaterialAssociations;

        public IEnumerable<Material> GetMaterials()
        {
            foreach (var item in _keyAndMaterialAssociations)
            {
                yield return item.UnityMaterial;
            }
        }

        public Material GetMaterialByKeyHash(int materialKeyHash)
        {
            if (_keyHashAndMaterialAssociations.TryGetValue(materialKeyHash, out var value))
            {
                return value;
            }

            throw new ArgumentException(
                $"There no material associated with hash: {materialKeyHash}", $"{nameof(materialKeyHash)}");
        }

        public bool IsAssociationExist(int materialKeyHash)
        {
            return _keyHashAndMaterialAssociations.ContainsKey(materialKeyHash);
        }
    }
}
