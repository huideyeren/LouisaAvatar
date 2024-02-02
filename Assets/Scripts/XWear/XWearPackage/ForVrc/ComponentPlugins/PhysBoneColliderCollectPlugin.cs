using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;
using XWear.IO.XResource;
using XWear.IO.XResource.Archive;
using XWear.IO.XResource.Component;
using XWear.IO.XResource.Component.ComponentPlugin;
using XWear.IO.XResource.Material;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.SwingParameter;
using XWear.IO.XResource.SwingParameter.PhysBone;
using XWear.IO.XResource.Transform;

namespace XWear.XWearPackage.ForVrc.ComponentPlugins
{
    public class PhysBoneColliderCollectPlugin
        : DefaultCollectComponentPluginBase<VRCPhysBoneCollider>
    {
        public override int Order => 10;

        protected override void CopyComponent(
            Transform attachTarget,
            VRCPhysBoneCollider sourceComponent
        )
        {
            var resultVrcPhysBoneCollider =
                attachTarget.gameObject.AddComponent<VRCPhysBoneCollider>();
            resultVrcPhysBoneCollider.rootTransform = sourceComponent.rootTransform;

            resultVrcPhysBoneCollider.shapeType = sourceComponent.shapeType;
            resultVrcPhysBoneCollider.radius = sourceComponent.radius;
            resultVrcPhysBoneCollider.height = sourceComponent.height;
            resultVrcPhysBoneCollider.position = sourceComponent.position;
            resultVrcPhysBoneCollider.rotation = sourceComponent.rotation;
            resultVrcPhysBoneCollider.insideBounds = sourceComponent.insideBounds;
            resultVrcPhysBoneCollider.bonesAsSpheres = sourceComponent.bonesAsSpheres;
        }

        public override IXResourceComponent Collect(
            XItem xItem,
            MaterialCollectorBase materialCollector,
            GameObjectWithTransformCollector gameObjectCollector,
            SkinnedMeshRendererDataCollector skinnedMeshRendererCollector,
            XResourceContainerUtil.XResourceArchiver archiver
        )
        {
            var vrcPhysBoneCollider = Context.Get();
            var param = new PhysBoneColliderParameter();
            param.rootTransformRef =
                CollectUtilForVrcProject.GetCollectedTransformGuidWithCheckNull(
                    vrcPhysBoneCollider.rootTransform,
                    gameObjectCollector
                );

            if (
                gameObjectCollector.GuidToXResourceGameObjectMemo.TryGetValue(
                    param.rootTransformRef,
                    out var xResourceGameObject
                )
            )
            {
                param.isAtHumanoidBone = xResourceGameObject.IsHumanoidBone;
                if (param.isAtHumanoidBone)
                {
                    param.humanBodyBones = xResourceGameObject.HumanBodyBones;
                }
            }

            param.shapeType = (int)vrcPhysBoneCollider.shapeType;
            param.radius = vrcPhysBoneCollider.radius;
            param.height = vrcPhysBoneCollider.height;
            param.position = vrcPhysBoneCollider.position;
            param.rotation = vrcPhysBoneCollider.rotation;
            param.insideBounds = vrcPhysBoneCollider.insideBounds;
            param.bonesAsSpheres = vrcPhysBoneCollider.bonesAsSpheres;

            return new XResourcePhysBoneCollider() { PhysBoneColliderParameter = param };
        }
    }
}
