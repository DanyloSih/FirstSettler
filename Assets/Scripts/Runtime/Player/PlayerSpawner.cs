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

        [SerializeField] private ObjectAppearanceObserver _playerPrefab;
        [SerializeField] private Transform _spawnPoint;

        private ObjectAppearanceObserver _playerInstance;

        public void RespawnPlayer()
        {
            if (_playerInstance != null)
            {
                Destroy(_playerInstance.gameObject);
                _playerInstance = null;
            }

            _playerInstance = _diContainer
                .InstantiatePrefabForComponent<ObjectAppearanceObserver>(
                _playerPrefab, _spawnPoint, new object[] { _playerListners });

            //_playerInstance.AddListners(_playerListners);
        }
    }

}
