using System.Collections.Generic;
using UnityEngine;
using Utilities.GameObjects;
using Zenject;

namespace FirstSettler.Player
{
    public class PlayerSpawner : MonoBehaviour
    {
        [Inject] private DiContainer _diContainer;
        [InjectOptional] private List<IObjectAppearanceListner> _playerListners;

        [SerializeField] private bool _findSpawnHeightViaRaycast = true;
        [SerializeField] private ObjectAppearanceObserver _playerPrefab;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private Vector3 _spawnOffset;

        private ObjectAppearanceObserver _playerInstance;

        public void RespawnPlayer()
        {
            if (_playerInstance != null)
            {
                Destroy(_playerInstance.gameObject);
                _playerInstance = null;
            }

            Vector3 spawnPosition = _spawnPoint.position;

            if (_findSpawnHeightViaRaycast)
            {
                List<RaycastHit> raycastHits = new List<RaycastHit>();
                raycastHits.AddRange(Physics.RaycastAll(_spawnPoint.position + Vector3.up * 10000, Vector3.down));

                if (raycastHits.Count > 0)
                {
                    spawnPosition.y = GetMaxHeight(raycastHits, raycastHits[0].point.y);
                }
            }

            spawnPosition += _spawnOffset;

            _playerInstance = _diContainer
                .InstantiatePrefabForComponent<ObjectAppearanceObserver>(
                _playerPrefab, _spawnPoint, new object[] { _playerListners });

            _playerInstance.transform.position = spawnPosition;
        }

        private float GetMaxHeight(List<RaycastHit> raycastHits, float defaultValue)
        {
            float maxHeight = defaultValue;

            foreach (var hit in raycastHits)
            {
                if (hit.point.y > maxHeight)
                {
                    maxHeight = hit.point.y;
                }
            }

            return maxHeight;
        }
    }

}
