using UnityEngine;

namespace XWear.IO.XResource.Humanoid.Estimate
{
    public partial class HumanoidEstimateUtil
    {
        private static EstimatePattern[] GenerateRightSideEstimatePatterns()
        {
            return new EstimatePattern[]
            {
                new EstimatePattern(
                    HumanBodyBones.RightShoulder,
                    GenerateSidePatterMatchFunction(HumanBodyBones.RightShoulder, split: false)
                ),
                new EstimatePattern(
                    HumanBodyBones.RightUpperArm,
                    GenerateSidePatterMatchFunction(HumanBodyBones.RightUpperArm)
                ),
                new EstimatePattern(
                    HumanBodyBones.RightLowerArm,
                    GenerateSidePatterMatchFunction(HumanBodyBones.RightLowerArm)
                ),
                new EstimatePattern(
                    HumanBodyBones.RightHand,
                    GenerateSidePatterMatchFunction(HumanBodyBones.RightHand, split: false)
                ),
                new EstimatePattern(
                    HumanBodyBones.RightUpperLeg,
                    GenerateSidePatterMatchFunction(HumanBodyBones.RightUpperLeg)
                ),
                new EstimatePattern(
                    HumanBodyBones.RightLowerLeg,
                    GenerateSidePatterMatchFunction(HumanBodyBones.RightLowerLeg)
                ),
                new EstimatePattern(
                    HumanBodyBones.RightFoot,
                    GenerateSidePatterMatchFunction(HumanBodyBones.RightFoot, split: false)
                ),
                new EstimatePattern(
                    HumanBodyBones.RightToes,
                    GenerateSidePatterMatchFunction(HumanBodyBones.RightToes, split: false)
                ),
                new EstimatePattern(
                    HumanBodyBones.RightIndexProximal,
                    GenerateSidePatterMatchFunction(HumanBodyBones.RightIndexProximal)
                ),
                new EstimatePattern(
                    HumanBodyBones.RightIndexIntermediate,
                    GenerateSidePatterMatchFunction(HumanBodyBones.RightIndexIntermediate)
                ),
                new EstimatePattern(
                    HumanBodyBones.RightIndexDistal,
                    GenerateSidePatterMatchFunction(HumanBodyBones.RightIndexDistal)
                ),
                new EstimatePattern(
                    HumanBodyBones.RightMiddleProximal,
                    GenerateSidePatterMatchFunction(HumanBodyBones.RightMiddleProximal)
                ),
                new EstimatePattern(
                    HumanBodyBones.RightMiddleIntermediate,
                    GenerateSidePatterMatchFunction(HumanBodyBones.RightMiddleIntermediate)
                ),
                new EstimatePattern(
                    HumanBodyBones.RightMiddleDistal,
                    GenerateSidePatterMatchFunction(HumanBodyBones.RightMiddleDistal)
                ),
                new EstimatePattern(
                    HumanBodyBones.RightRingProximal,
                    GenerateSidePatterMatchFunction(HumanBodyBones.RightRingProximal)
                ),
                new EstimatePattern(
                    HumanBodyBones.RightRingIntermediate,
                    GenerateSidePatterMatchFunction(HumanBodyBones.RightRingIntermediate)
                ),
                new EstimatePattern(
                    HumanBodyBones.RightRingDistal,
                    GenerateSidePatterMatchFunction(HumanBodyBones.RightRingDistal)
                ),
                new EstimatePattern(
                    HumanBodyBones.RightLittleProximal,
                    GenerateSidePatterMatchFunction(HumanBodyBones.RightLittleProximal)
                ),
                new EstimatePattern(
                    HumanBodyBones.RightLittleIntermediate,
                    GenerateSidePatterMatchFunction(HumanBodyBones.RightLittleIntermediate)
                ),
                new EstimatePattern(
                    HumanBodyBones.RightLittleDistal,
                    GenerateSidePatterMatchFunction(HumanBodyBones.RightLittleDistal)
                ),
                new EstimatePattern(
                    HumanBodyBones.RightThumbProximal,
                    GenerateSidePatterMatchFunction(HumanBodyBones.RightThumbProximal)
                ),
                new EstimatePattern(
                    HumanBodyBones.RightThumbIntermediate,
                    GenerateSidePatterMatchFunction(HumanBodyBones.RightThumbIntermediate)
                ),
                new EstimatePattern(
                    HumanBodyBones.RightThumbDistal,
                    GenerateSidePatterMatchFunction(HumanBodyBones.RightThumbDistal)
                )
            };
        }
    }
}
