using VRC.SDK3.Dynamics.PhysBone.Components;
using XWear.IO;
using XWear.IO.Avatar.HumanoidTransfer;
using XWear.IO.XResource.Util;

namespace XWear.XWearPackage.ForVrc.HumanoidTransfer
{
    public class PhysBoneHumanoidTransfer : DefaultHumanoidTransferBase<VRCPhysBone>
    {
        public override int Order => 11;

        protected override void Transfer(
            VRCPhysBone component,
            HumanoidMap baseModelHumanoidMap,
            HumanoidMap xResourceHumanoidMap
        )
        {
            var rootTransform = component.rootTransform;
            if (rootTransform == null)
            {
                return;
            }

            if (
                xResourceHumanoidMap.GetMap.TryGetValue(rootTransform, out var targetHumanBodyBones)
            )
            {
                if (
                    baseModelHumanoidMap.GetMap
                        .FlipKvp()
                        .TryGetValue(targetHumanBodyBones, out var baseModelBone)
                )
                {
                    rootTransform = baseModelBone;
                    // todo 位置計算
                }
            }

            component.rootTransform = rootTransform;
        }
    }
}
