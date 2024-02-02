using System;
using UnityEngine;

namespace XWear.IO
{
    public class AccessoryMapComponent : MonoBehaviour
    {
        [SerializeField]
        private AccessoryMap accessoryMap;
        public AccessoryMap AccessoryMap => accessoryMap;

        public void LoadAccessoryMap(AccessoryMap from)
        {
            accessoryMap = from;
        }
    }

    [Serializable]
    public class AccessoryMap
    {
        public HumanBodyBones[] recommendHumanBodyBones;
    }
}
