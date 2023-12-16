using System;
using UnityEngine;
using System.Linq;

namespace FirstSettler.Common
{
    public class CommonRaycastPointer : MonoBehaviour
    {
        [SerializeField] private Camera _rayThrowerCamera;
        [SerializeField] private float _maxRaycastDistance;

        private Ray _currentThrowingRay;
        private RaycastHit[] _currentRaycastHits;

        public Ray CurrentThrowingRay { get => _currentThrowingRay; }
        /// <summary>
        /// Raycast hits for the last physics frame, sorted 
        /// by increasing distance from the camera.
        /// </summary>
        public RaycastHit[] CurrentRaycastHitsSorted { get => _currentRaycastHits; }
        public float MaxRaycastDistance { get => _maxRaycastDistance; }

        public event Action<RaycastHit[]> RaycastHitsUpdated;

        private void FixedUpdate()
        {
            _currentThrowingRay = _rayThrowerCamera.ScreenPointToRay(
                new Vector3(Screen.width / 2, Screen.height / 2, 0));

            _currentRaycastHits = Physics.RaycastAll(_currentThrowingRay, _maxRaycastDistance);
            var throwingRayOrigin = _currentThrowingRay.origin;
            SortByAscendingDistance(throwingRayOrigin);
            RaycastHitsUpdated?.Invoke(_currentRaycastHits);
        }

        private void SortByAscendingDistance(Vector3 throwingRayOrigin)
        {
            Array.Sort(_currentRaycastHits, (hit1, hit2) =>
            {
                float distance1 = Vector3.Distance(throwingRayOrigin, hit1.point);
                float distance2 = Vector3.Distance(throwingRayOrigin, hit2.point);
                return distance1.CompareTo(distance2);
            });
        }
    }
}
