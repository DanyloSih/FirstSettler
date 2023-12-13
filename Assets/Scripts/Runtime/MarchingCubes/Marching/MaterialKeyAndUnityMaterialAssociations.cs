using System;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingCubesProject
{
    [CreateAssetMenu(
        fileName = nameof(MaterialKeyAndUnityMaterialAssociations),
        menuName = "FirstSettler/MarchingCubes/" + nameof(MaterialKeyAndUnityMaterialAssociations))]
    public class MaterialKeyAndUnityMaterialAssociations : ScriptableObject
    {
        [SerializeField] private GenericDictionary<MaterialKey, Material> _keyAndMaterialAssociations;

        private Dictionary<int, Material> _keyHashAndMaterialAssociations = new Dictionary<int, Material>();

        protected void OnEnable()
        {
            _keyHashAndMaterialAssociations.Clear();
            foreach (var item in _keyAndMaterialAssociations)
            {
                _keyHashAndMaterialAssociations.Add(item.Key.GetHashCode(), item.Value);
            }
        }

        public IEnumerable<KeyValuePair<int, Material>> GetMaterialKeyHashAndMaterialAssociations()
            => _keyHashAndMaterialAssociations;

        public IEnumerable<Material> GetMaterials()
            => _keyAndMaterialAssociations.Values;

        public Material GetMaterialByKeyHash(int materialKeyHash)
        {
            if (_keyHashAndMaterialAssociations.TryGetValue(materialKeyHash, out var value))
            {
                return value;
            }

            throw new ArgumentException(
                $"There no material associated with hash: {materialKeyHash}", $"{nameof(materialKeyHash)}");
        }

        public Material GetUnityMaterialByMaterialKey(MaterialKey materialKey)
        {
            if (_keyAndMaterialAssociations.TryGetValue(materialKey, out var value))
            {
                return value;
            }

            throw new ArgumentException(
                $"There no material associated with {nameof(MaterialKey)}: " +
                $"{materialKey.MaterialName}", $"{nameof(materialKey)}");
        }
    }
}
