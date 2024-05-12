using System;
using System.Runtime.CompilerServices;

namespace Utilities.Common
{
    public class DisposableCollectionManager<T> : IDisposable
        where T : IDisposable
    {
        private Func<int, T> _createObjectFunction;
        private T _disposableObject;
        private int _count;

        public int ElementsCount { get => _count; }
        public T CashedObject { get => _disposableObject; }

        public DisposableCollectionManager(Func<int, T> createObjectFunction)
        {
            _createObjectFunction = createObjectFunction;
        }

        public T GetObjectInstance(int elementsCount)
        {
            if (!DisposeCondition(_disposableObject))
            {
                CreateObject(elementsCount);
                return _disposableObject;
            }
            else if (_count != elementsCount)
            {
                Dispose();
                CreateObject(elementsCount);
            }

            return _disposableObject;
        }

        public void Dispose()
        {
            if (DisposeCondition(_disposableObject))
            {
                _disposableObject.Dispose();
                _disposableObject = default;
                _count = 0;
            }
        }

        protected virtual bool DisposeCondition(T disposableObject)
        {
            return disposableObject != null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CreateObject(int count)
        {
            _disposableObject = _createObjectFunction(count);
            _count = count;
        }
    }

}