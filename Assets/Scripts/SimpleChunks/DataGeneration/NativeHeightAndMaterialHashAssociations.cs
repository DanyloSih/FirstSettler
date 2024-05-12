using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;

namespace SimpleChunks.DataGeneration
{
    public struct NativeHeightAndMaterialHashAssociations : IDisposable
    {
        private NativeArray<KeyValuePair<float, int>> _associations;
        private KeyValuePair<float, int> _minAssociation;
        private KeyValuePair<float, int> _maxAssociation;
        private int _associationsCount;
        private bool _isDisposed;

        public bool IsDisposed { get => _isDisposed || !_associations.IsCreated; }

        public NativeHeightAndMaterialHashAssociations(
            NativeArray<KeyValuePair<float, int>> associations,
            KeyValuePair<float, int> minAssociation,
            KeyValuePair<float, int> maxAssociation)
        {
            _associations = associations;
            _minAssociation = minAssociation;
            _maxAssociation = maxAssociation;
            _associationsCount = _associations.Length;
            _isDisposed = false;
        }

        public int GetMaterialHashByHeight(float height)
        {
            if (_associationsCount == 0)
            {
                return 0;
            }
            else if (height >= _maxAssociation.Key)
            {
                return _maxAssociation.Value;
            }
            else if (height <= _minAssociation.Key)
            {
                return _minAssociation.Value;
            }
            else
            {
                int result = 0;
                for (int i = _associationsCount - 1; i >= 0; i--)
                {
                    KeyValuePair<float, int> association = _associations[i];

                    if (association.Key > height)
                    {
                        result = association.Value;
                        break;
                    }
                }

                return result;
            }
        }

        public void Dispose()
        {
            _associations.Dispose();
            _isDisposed = true;
        }
    }
}
