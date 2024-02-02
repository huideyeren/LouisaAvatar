using UnityEngine;

namespace XWear.IO.Avatar
{
    public class ExportSourceBaseModel
    {
        private readonly GameObject _sourceGameObject;
        public GameObject CopiedBaseModelGameObject { get; private set; }
        public HumanoidMap CopiedBaseModelHumanoidMap { get; private set; }
        private readonly Transform _exportRoot;

        public ExportSourceBaseModel(GameObject sourceGameObject, Transform exportRoot, string name)
        {
            _sourceGameObject = sourceGameObject;
            _exportRoot = exportRoot;
            CopiedBaseModelGameObject = CopyBaseModel(sourceGameObject, name);
            CopiedBaseModelHumanoidMap = CopyHumanoidMap();
        }

        public void Validate()
        {
            if (_sourceGameObject == null)
            {
                Destroy();
            }
        }

        private GameObject CopyBaseModel(GameObject sourceBaseModelGameObject, string name)
        {
            var copiedSourceBaseModelGameObject = Object.Instantiate(
                sourceBaseModelGameObject,
                parent: _exportRoot
            );
            copiedSourceBaseModelGameObject.name = name;
            var smrs =
                copiedSourceBaseModelGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var smr in smrs)
            {
                var smrTransform = smr.transform;
                smrTransform.localPosition = Vector3.zero;
                smrTransform.localRotation = Quaternion.identity;
                smrTransform.localScale = Vector3.one;
            }

            return copiedSourceBaseModelGameObject;
        }

        private HumanoidMap CopyHumanoidMap()
        {
            var humanoidMapComponent =
                CopiedBaseModelGameObject.GetComponent<HumanoidMapComponent>();
            if (humanoidMapComponent == null)
            {
                humanoidMapComponent =
                    CopiedBaseModelGameObject.AddComponent<HumanoidMapComponent>();
                humanoidMapComponent.AutoAssign();
            }

            return humanoidMapComponent.HumanoidMap;
        }

        public void Recreate()
        {
            Destroy();
            CopiedBaseModelGameObject = CopyBaseModel(_sourceGameObject, _sourceGameObject.name);
            CopiedBaseModelHumanoidMap = CopyHumanoidMap();
        }

        public void Destroy()
        {
            if (CopiedBaseModelGameObject != null)
            {
                Object.DestroyImmediate(CopiedBaseModelGameObject);
            }
        }
    }
}
