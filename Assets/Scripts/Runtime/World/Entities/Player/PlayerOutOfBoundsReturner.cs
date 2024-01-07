using UnityEngine;

namespace World.Entities.Player
{
	public class PlayerOutOfBoundsReturner : MonoBehaviour
	{
		[SerializeField] private Transform _playerTransform;
		[SerializeField] private float _minWorldY;

        private Vector3 _initializePosition;

        protected void Start() 
		{
			_initializePosition = _playerTransform.position;
		}

		protected void FixedUpdate()
		{
			if (_playerTransform.position.y <= _minWorldY)
			{
                _playerTransform.position = _initializePosition;
			}
		}
	} 
}
