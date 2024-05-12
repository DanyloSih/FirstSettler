using UnityEngine;

namespace Utilities.GameObjects
{

    public interface IObjectAppearanceListner
    {
        public void OnObjectAppeared(GameObject gameObject);

        public void OnObjectDisappeared();
    }
}
