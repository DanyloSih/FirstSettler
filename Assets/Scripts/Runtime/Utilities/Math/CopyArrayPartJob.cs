using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Utilities.Math
{
    public struct CopyArrayPartJob<T> : IJobParallelFor
        where T : struct
    {
        [ReadOnly]
        private ThreedimensionalNativeArray<T> _donor;
        [WriteOnly]
        private ThreedimensionalNativeArray<T> _result;
        private Vector3Int _minPosition;
        private RectPrismInt _resultParallelepiped;

        /// <param name="donor">The array FROM which the data will be copied.</param>
        /// <param name="result">The array TO which the data will be copied</param>
        /// <param name="minPosition">The starting position inside the donor from which data copying will begin.</param>
        public CopyArrayPartJob(
            ThreedimensionalNativeArray<T> donor,
            ThreedimensionalNativeArray<T> result,
            Vector3Int minPosition)
        {
            _donor = donor;
            _result = result;
            _minPosition = minPosition;
            _resultParallelepiped = _result.RectPrism;
        }

        public void Execute(int index)
        {
            Vector3Int inResultPosition = _resultParallelepiped.IndexToPoint(index);
            Vector3Int inDonorPosition = inResultPosition + _minPosition;

            T value = _donor.GetValue(inDonorPosition.x, inDonorPosition.y, inDonorPosition.z);
            _result.SetValue(index, value);

            //if (_result.Parallelepiped.IsContainsPoint(inResultPosition)
            // && _donor.Parallelepiped.IsContainsPoint(inDonorPosition))
            //{
            //}
        }
    }
}
