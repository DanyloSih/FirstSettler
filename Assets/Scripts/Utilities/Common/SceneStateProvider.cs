using UnityEngine;

namespace Utilities.Common
{
    public class SceneStateProvider : MonoBehaviour
    {
        public bool IsAwake {  get; private set; }
        public bool IsStart {  get; private set; }
        public bool IsOnEnable {  get; private set; }
        public bool IsOnDisable {  get; private set; }
        public bool IsOnDestroy {  get; private set; }

        public void Awake()
        {
            SetAllToFalse();
            IsAwake = true;
        }

        public void Start()
        {
            SetAllToFalse();
            IsStart = true;
        }

        public void OnEnable()
        {
            SetAllToFalse();
            IsOnEnable = true;
        }

        public void OnDisable()
        {
            SetAllToFalse();
            IsOnDisable = true;
        }

        public void OnDestroy()
        {
            SetAllToFalse();
            IsOnDestroy = true;
        }

        private void SetAllToFalse()
        {
            IsAwake = false;
            IsStart = false;
            IsOnEnable = false;
            IsOnDisable = false;
            IsOnDestroy = false;
        }
    }

}