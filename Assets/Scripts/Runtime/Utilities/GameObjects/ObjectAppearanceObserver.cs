using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Utilities.GameObjects
{
    public class ObjectAppearanceObserver : MonoBehaviour
    {
        enum ObservingMethods
        {
            OnEnable_OnDisable,
            Awake_OnDestroy
        }

        [SerializeField] private ObservingMethods _observingMethods;
        [SerializeField] private GameObject _observingGameObject;

        private List<IObjectAppearanceListner> _objectAppearanceListners = new();

        [Inject]
        public void AddListners([InjectOptional] List<IObjectAppearanceListner> objectAppearanceListners)
        {
            if (objectAppearanceListners == null || objectAppearanceListners.Count == 0)
            {
                return;
            }

            _objectAppearanceListners.AddRange(objectAppearanceListners);
        }

        public void AddListner(IObjectAppearanceListner objectAppearanceListner)
        {
            _objectAppearanceListners.Add(objectAppearanceListner);
        }

        public void RemoveListner(IObjectAppearanceListner objectAppearanceListner)
        {
            _objectAppearanceListners.Remove(objectAppearanceListner);
        }

        protected void Awake()
        {
            if (_observingMethods == ObservingMethods.Awake_OnDestroy)
            {
                _objectAppearanceListners.ForEach(x => x.OnObjectAppeared(_observingGameObject));
            }
        }

        protected void OnEnable()
        {
            if(_observingMethods == ObservingMethods.OnEnable_OnDisable)
            {
                _objectAppearanceListners.ForEach(x => x.OnObjectAppeared(_observingGameObject));
            }
        }

        protected void OnDisable()
        {
            if (_observingMethods == ObservingMethods.OnEnable_OnDisable)
            {
                _objectAppearanceListners.ForEach(x => x.OnObjectDisappeared());
            }
        }

        protected void OnDestroy()
        {
            if (_observingMethods == ObservingMethods.Awake_OnDestroy)
            {
                _objectAppearanceListners.ForEach(x => x.OnObjectDisappeared());
            }
        }
    }
}
