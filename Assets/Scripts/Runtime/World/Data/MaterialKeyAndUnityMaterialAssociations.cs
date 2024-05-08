using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Utilities.Jobs;
using Zenject;

namespace World.Data
{
    [CreateAssetMenu(
        fileName = nameof(MaterialKeyAndUnityMaterialAssociations),
        menuName = "FirstSettler/MarchingCubes/" + nameof(MaterialKeyAndUnityMaterialAssociations))]
    public class MaterialKeyAndUnityMaterialAssociations : ScriptableObject, IInitializable, IDisposable
    {
        [SerializeField] private List<KeyAndMaterialAssociation> _keyAndMaterialAssociations;

        [NonSerialized] private Dictionary<int, Material> _keyHashAndMaterialAssociations = new Dictionary<int, Material>();
        [NonSerialized] private NativeHashSetManager<int> _keysHashSetManager = new NativeHashSetManager<int>(
            (count) => new NativeHashSet<int>(count, Allocator.Persistent));

        public int Count => _keyAndMaterialAssociations.Count;

        public NativeHashSet<int> GetKeysHashSet()
        {
            var hashSet = _keysHashSetManager.GetObjectInstance(_keyAndMaterialAssociations.Count);

            if (_keyAndMaterialAssociations.Count > 0
            && !hashSet.Contains(_keyAndMaterialAssociations[0].MaterialKey.GetHashCode()))
            {
                hashSet.Clear();
                foreach (var item in _keyAndMaterialAssociations)
                {
                    hashSet.Add(item.MaterialKey.GetHashCode());
                }
            }

            return hashSet;
        }

        public void Initialize()
        {
            _keyHashAndMaterialAssociations.Clear();
            foreach (var item in _keyAndMaterialAssociations)
            {
                _keyHashAndMaterialAssociations.Add(item.MaterialKey.GetHashCode(), item.UnityMaterial);
            }
        }

        public void Dispose()
        {
            _keysHashSetManager.Dispose();
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
