using System;
using System.Collections.Generic;
using UnityEngine;

namespace World.Data
{
    [CreateAssetMenu(
    fileName = nameof(MaterialKeyAndHeightAssociations),
    menuName = "FirstSettler/MarchingCubes/" + nameof(MaterialKeyAndHeightAssociations))]
    public class MaterialKeyAndHeightAssociations : ScriptableObject
    {
        [SerializeField] private List<HeightAndMaterialKeyAssociation> _heightAndMaterialKeyAssociations;

        private List<KeyValuePair<float, int>> _heightAndMaterialKeyHashAssociations;
        private KeyValuePair<float, int>? _minAssociation;
        private KeyValuePair<float, int>? _maxAssociation;

        [NonSerialized] private bool _isInitialized = false;

        public int Count => _heightAndMaterialKeyAssociations.Count;
        public KeyValuePair<float, int>? MinAssociation { get => _minAssociation; }
        public KeyValuePair<float, int>? MaxAssociation { get => _maxAssociation; }

        protected void OnEnable()
        {
            Initialize();
        }

        public IEnumerable<HeightAndMaterialKeyAssociation> GetEnumerable()
        {
            return _heightAndMaterialKeyAssociations;
        }

        public int GetMaterialKeyHashByHeight(float height)
        {
            if (height >= _maxAssociation.Value.Key)
            {
                return _maxAssociation.Value.Value;
            }
            else if (height <= _minAssociation.Value.Key)
            {
                return _minAssociation.Value.Value;
            }
            else
            {
                return _heightAndMaterialKeyHashAssociations
                       .FindLast(association => association.Key > height).Value;
            }
        }

        public void Initialize()
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                SortHeightAndMaterialKeyAssociations();
                ClearOrCreateAssociations();
                InititalizeAssociationsWithHash();
            }
        }

        private void InititalizeAssociationsWithHash()
        {
            _minAssociation = null;
            _maxAssociation = null;
            foreach (var item in _heightAndMaterialKeyAssociations)
            {
                var association = new KeyValuePair<float, int>(item.Height, item.MaterialKey.GetHashCode());
                _heightAndMaterialKeyHashAssociations.Add(association);

                if (_minAssociation == null || _minAssociation.Value.Key > association.Key)
                {
                    _minAssociation = association;
                }

                if (_maxAssociation == null || _maxAssociation.Value.Key < association.Key)
                {
                    _maxAssociation = association;
                }
            }
            _minAssociation = _minAssociation ?? new KeyValuePair<float, int>();
            _maxAssociation = _maxAssociation ?? new KeyValuePair<float, int>();
        }

        private void ClearOrCreateAssociations()
        {
            if (_heightAndMaterialKeyHashAssociations == null)
            {
                _heightAndMaterialKeyHashAssociations = new List<KeyValuePair<float, int>>();
            }
            else
            {
                _heightAndMaterialKeyHashAssociations.Clear();
            }
        }

        [ContextMenu("Sort associations")]
        private void SortHeightAndMaterialKeyAssociations()
        {
            _heightAndMaterialKeyAssociations.Sort((a, b) => b.Height.CompareTo(a.Height));
        }
    }
}
