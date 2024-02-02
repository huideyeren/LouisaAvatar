using UnityEngine;

namespace VirtualLens2
{

    public class ProjectSettings : ScriptableObject
    {
        public int targetHand = 0;
        public GameObject settingsTemplate = null;
        public bool useCustomModel = false;
        public GameObject customModelPrefab = null;
        public bool keepCustomModelTransform = false;
    }

}