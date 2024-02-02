using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.PhysBone.Components;
using XWear.IO;
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
    public class PhysBoneColliderBuildPlugin
        : DefaultBuildComponentPluginBase<XResourcePhysBoneCollider>
    {
        public override int Order => 10;

        public override Component BuildAndAttach(
            GameObjectWithTransformBuilder gameObjectBuilder,
            SkinnedMeshRendererDataBuilder skinnedMeshRendererBuilder,
            MaterialBuilderBase materialBuilder,
            GameObject attachTarget,
            XResourceContainerUtil.XResourceOpener opener,
            AssetSaver assetSaver
        )
        {
            var vrcPhysBoneCollider = attachTarget.AddComponent<VRCPhysBoneCollider>();
            var param = Context.Get().PhysBoneColliderParameter;

            vrcPhysBoneCollider.rootTransform =
                BuildUtilForVrcProject.GetBuiltTransformWithCheckStringEmpty(
                    param.rootTransformRef,
                    gameObjectBuilder
                );

            #region Shape

            vrcPhysBoneCollider.shapeType = (VRCPhysBoneColliderBase.ShapeType)param.shapeType;
            vrcPhysBoneCollider.radius = param.radius;
            vrcPhysBoneCollider.height = param.height;
            vrcPhysBoneCollider.position = param.position;
            vrcPhysBoneCollider.rotation = param.rotation;
            vrcPhysBoneCollider.insideBounds = param.insideBounds;
            vrcPhysBoneCollider.bonesAsSpheres = param.bonesAsSpheres;

            #endregion

            return vrcPhysBoneCollider;
        }
    }
}
