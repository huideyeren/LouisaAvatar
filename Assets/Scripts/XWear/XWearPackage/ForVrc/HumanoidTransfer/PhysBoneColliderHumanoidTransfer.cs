using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;
using XWear.IO;
using XWear.IO.Avatar.HumanoidTransfer;
using XWear.IO.XResource.Util;

namespace XWear.XWearPackage.ForVrc.HumanoidTransfer
{
    public class PhysBoneColliderHumanoidTransfer : DefaultHumanoidTransferBase<VRCPhysBoneCollider>
    {
        public override int Order => 11;

        protected override void Transfer(
            VRCPhysBoneCollider component,
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
                    // PhysBoneColliderはPhysBoneCollider.rootTransformからみたローカル座標系での移動と回転を持つ
                    // 素体モデルと衣装モデルではHumanoidBoneで回転軸の違いが存在する可能性があるため、それを考慮して
                    // 対象の素体モデルのHumanoidBoneからみたローカル座標系の位置と回転を計算する

                    // 回転計算
                    var sourceColliderLocalRotation = component.rotation;
                    // もともとのColliderの回転量(ワールド座標系)
                    var sourceColliderWorldRotation =
                        rootTransform.rotation * sourceColliderLocalRotation;
                    // 素体ボーンからみた回転量を求める(ローカル座標系)
                    var resultRotation =
                        Quaternion.Inverse(baseModelBone.rotation) * sourceColliderWorldRotation;
                    component.rotation = resultRotation;

                    // 位置計算
                    var sourceColliderPosition = component.position;
                    // もともとのCollider位置のワールド座標
                    var sourceColliderWorldPosition =
                        rootTransform.localToWorldMatrix.MultiplyPoint(sourceColliderPosition);

                    // もともとのCollider位置の移動量(ワールド座標)
                    var sourceWorldPositionDiff =
                        sourceColliderWorldPosition - rootTransform.position;
                    // もともとのCollider位置の移動量を素体側ボーン位置(ワールド座標)に足して、素体側でのワールド座標を出す
                    var resultWorldPosition = baseModelBone.position + sourceWorldPositionDiff;

                    // 素体モデルのボーン座標ではなく、衣装モデルのボーン座標を尊重する場合はこっちだが、大抵の場合は素体モデル側でよさそう
                    //var resultWorldPosition = sourceColliderPosition;

                    // 素体側でのワールド座標を素体ボーンからみたローカル座標に変換
                    var baseModelColliderLocalPosition =
                        baseModelBone.worldToLocalMatrix.MultiplyPoint(resultWorldPosition);

                    component.position = baseModelColliderLocalPosition;
                    rootTransform = baseModelBone;
                }
            }

            component.rootTransform = rootTransform;
        }
    }
}
