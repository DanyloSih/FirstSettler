using System;
using System.Collections;
using UnityEngine;
using Utilities.GameObjects;
using World.Data;
using Zenject;
using Utilities.UnityObjects.Extensions;
using Utilities.Math;

namespace World.Organization
{
    public class ChunkPositionProvider : MonoBehaviour, IObjectAppearanceListner
    {
        [Tooltip("How often will the script check for changes in generation point.")]
        [SerializeField] private float _chunkCheckDelay = 0.5f;

        private Transform _targetTransform;
        private Coroutine _targetPositionCheckCoroutine;
        private BasicChunkSettings _basicChunkSettings;
        private ChunkCoordinatesCalculator _chunkCoordinatesCalculator;
        private Vector3Int _previousTargetChunkPosition;

        public event Action<Vector3Int> ChunkPositionChanged;

        public Vector3Int CashedChunkPosition { get => _previousTargetChunkPosition; }

        [Inject]
        public void Construct(BasicChunkSettings basicChunkSettings)
        {
            _basicChunkSettings = basicChunkSettings;
            _chunkCoordinatesCalculator = new ChunkCoordinatesCalculator(
                _basicChunkSettings.Size, _basicChunkSettings.Scale);
        }

        public Vector3Int CalculateTargetLocalChunkPosition()
        {
            Vector3 position = _targetTransform == null ? Vector3.zero : _targetTransform.position;

            return _chunkCoordinatesCalculator
                .GetLocalChunkPositionByGlobalPoint(position);
        }

        public void OnObjectAppeared(GameObject gameObject)
        {
            _targetTransform = gameObject.transform;

            if (_targetPositionCheckCoroutine == null)
            {
                _targetPositionCheckCoroutine = StartCoroutine(
                   TargetPositionCheckProcess(_chunkCheckDelay));
            }
        }

        public void OnObjectDisappeared()
        {
            if (_targetPositionCheckCoroutine != null && !this.IsNullOrDestroyed())
            {
                StopCoroutine(_targetPositionCheckCoroutine);
            }
            _targetTransform = null;
        }

        private IEnumerator TargetPositionCheckProcess(float delay)
        {
            _previousTargetChunkPosition = CalculateTargetLocalChunkPosition();

            yield return new WaitForSeconds(delay);

            while (true)
            {
                Vector3Int targetChunkPosition = CalculateTargetLocalChunkPosition();
                if (targetChunkPosition != _previousTargetChunkPosition)
                {
                    ChunkPositionChanged?.Invoke(targetChunkPosition);
                }

                _previousTargetChunkPosition = targetChunkPosition;
                yield return new WaitForSeconds(delay);
            }
        }
    }
}
