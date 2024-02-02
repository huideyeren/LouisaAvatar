using UnityEngine;

namespace XWear.IO.XResource.Humanoid.Estimate
{
    public partial class HumanoidEstimateUtil
    {
        private static EstimatePattern[] GenerateLeftSideEstimatePatterns()
        {
            return new EstimatePattern[]
            {
                new EstimatePattern(
                    HumanBodyBones.LeftShoulder,
                    GenerateSidePatterMatchFunction(HumanBodyBones.LeftShoulder, split: false)
                ),
                new EstimatePattern(
                    HumanBodyBones.LeftUpperArm,
                    GenerateSidePatterMatchFunction(HumanBodyBones.LeftUpperArm)
                ),
                new EstimatePattern(
                    HumanBodyBones.LeftLowerArm,
                    GenerateSidePatterMatchFunction(HumanBodyBones.LeftLowerArm)
                ),
                new EstimatePattern(
                    HumanBodyBones.LeftHand,
                    GenerateSidePatterMatchFunction(HumanBodyBones.LeftHand, split: false)
                ),
                new EstimatePattern(
                    HumanBodyBones.LeftUpperLeg,
                    GenerateSidePatterMatchFunction(HumanBodyBones.LeftUpperLeg)
                ),
                new EstimatePattern(
                    HumanBodyBones.LeftLowerLeg,
                    GenerateSidePatterMatchFunction(HumanBodyBones.LeftLowerLeg)
                ),
                new EstimatePattern(
                    HumanBodyBones.LeftFoot,
                    GenerateSidePatterMatchFunction(HumanBodyBones.LeftFoot, split: false)
                ),
                new EstimatePattern(
                    HumanBodyBones.LeftToes,
                    GenerateSidePatterMatchFunction(HumanBodyBones.LeftToes, split: false)
                ),
                new EstimatePattern(
                    HumanBodyBones.LeftIndexProximal,
                    GenerateSidePatterMatchFunction(HumanBodyBones.LeftIndexProximal)
                ),
                new EstimatePattern(
                    HumanBodyBones.LeftIndexIntermediate,
                    GenerateSidePatterMatchFunction(HumanBodyBones.LeftIndexIntermediate)
                ),
                new EstimatePattern(
                    HumanBodyBones.LeftIndexDistal,
                    GenerateSidePatterMatchFunction(HumanBodyBones.LeftIndexDistal)
                ),
                new EstimatePattern(
                    HumanBodyBones.LeftMiddleProximal,
                    GenerateSidePatterMatchFunction(HumanBodyBones.LeftMiddleProximal)
                ),
                new EstimatePattern(
                    HumanBodyBones.LeftMiddleIntermediate,
                    GenerateSidePatterMatchFunction(HumanBodyBones.LeftMiddleIntermediate)
                ),
                new EstimatePattern(
                    HumanBodyBones.LeftMiddleDistal,
                    GenerateSidePatterMatchFunction(HumanBodyBones.LeftMiddleDistal)
                ),
                new EstimatePattern(
                    HumanBodyBones.LeftRingProximal,
                    GenerateSidePatterMatchFunction(HumanBodyBones.LeftRingProximal)
                ),
                new EstimatePattern(
                    HumanBodyBones.LeftRingIntermediate,
                    GenerateSidePatterMatchFunction(HumanBodyBones.LeftRingIntermediate)
                ),
                new EstimatePattern(
                    HumanBodyBones.LeftRingDistal,
                    GenerateSidePatterMatchFunction(HumanBodyBones.LeftRingDistal)
                ),
                new EstimatePattern(
                    HumanBodyBones.LeftLittleProximal,
                    GenerateSidePatterMatchFunction(HumanBodyBones.LeftLittleProximal)
                ),
                new EstimatePattern(
                    HumanBodyBones.LeftLittleIntermediate,
                    GenerateSidePatterMatchFunction(HumanBodyBones.LeftLittleIntermediate)
                ),
                new EstimatePattern(
                    HumanBodyBones.LeftLittleDistal,
                    GenerateSidePatterMatchFunction(HumanBodyBones.LeftLittleDistal)
                ),
                new EstimatePattern(
                    HumanBodyBones.LeftThumbProximal,
                    GenerateSidePatterMatchFunction(HumanBodyBones.LeftThumbProximal)
                ),
                new EstimatePattern(
                    HumanBodyBones.LeftThumbIntermediate,
                    GenerateSidePatterMatchFunction(HumanBodyBones.LeftThumbIntermediate)
                ),
                new EstimatePattern(
                    HumanBodyBones.LeftThumbDistal,
                    GenerateSidePatterMatchFunction(HumanBodyBones.LeftThumbDistal)
                )
            };
        }
    }
}
