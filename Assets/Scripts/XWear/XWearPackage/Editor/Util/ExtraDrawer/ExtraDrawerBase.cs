using System;
using UnityEngine;

namespace XWear.XWearPackage.Editor.Util.ExtraDrawer
{
    [Serializable]
    public abstract class ExtraDrawerBase
    {
        [SerializeField]
        private static GameObject _targetCache;

        protected GameObject GetTarget(GameObject extraDrawTarget)
        {
            if (extraDrawTarget == _targetCache)
            {
                return _targetCache;
            }

            _targetCache = extraDrawTarget;
            OnCacheChanged(_targetCache);

            return _targetCache;
        }

        protected abstract void OnCacheChanged(GameObject newTarget);
    }
}
