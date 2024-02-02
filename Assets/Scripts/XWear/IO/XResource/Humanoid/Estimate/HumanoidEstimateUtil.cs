using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace XWear.IO.XResource.Humanoid.Estimate
{
    public static partial class HumanoidEstimateUtil
    {
        public class EstimatePattern
        {
            public readonly HumanBodyBones HumanBodyBones;
            public readonly Func<string, bool> CheckFunction;

            public EstimatePattern(HumanBodyBones humanBodyBones, Func<string, bool> checkFunction)
            {
                HumanBodyBones = humanBodyBones;
                CheckFunction = checkFunction;
            }
        }

        public static EstimatePattern[] GenerateEstimatePatterns()
        {
            var patterns = new List<EstimatePattern>();
            patterns.AddRange(GenerateBodyEstimatePatterns());
            patterns.AddRange(GenerateLeftSideEstimatePatterns());
            patterns.AddRange(GenerateRightSideEstimatePatterns());
            return patterns.ToArray();
        }

        private static EstimatePattern[] GenerateBodyEstimatePatterns()
        {
            return new EstimatePattern[]
            {
                new EstimatePattern(
                    HumanBodyBones.Hips,
                    GenerateSinglePatternMatchFunction(@"hips")
                ),
                new EstimatePattern(
                    HumanBodyBones.Spine,
                    GenerateSinglePatternMatchFunction(@"spine")
                ),
                new EstimatePattern(
                    HumanBodyBones.Chest,
                    GenerateSinglePatternMatchFunction(@"(?:upper)?chest")
                ),
                new EstimatePattern(
                    HumanBodyBones.Neck,
                    GenerateSinglePatternMatchFunction(@"neck")
                ),
                new EstimatePattern(
                    HumanBodyBones.Head,
                    GenerateSinglePatternMatchFunction(@"head")
                )
            };
        }

        private const string PatternL = "Left";
        private const string PatternR = "Right";

        private static Func<string, bool> GenerateSinglePatternMatchFunction(string pattern)
        {
            return boneName =>
            {
                var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                return regex.IsMatch(boneName) && !boneName.Contains("PB");
            };
        }

        private static Func<string, bool> GenerateSidePatterMatchFunction(
            HumanBodyBones humanBodyBones,
            bool split = true
        )
        {
            var humanBodyBonesRawString = humanBodyBones.ToString().TrimEnd('s');

            var sidePrefixPattern = humanBodyBonesRawString.StartsWith(PatternL)
                ? PatternL
                : PatternR;
            var sidePattern = humanBodyBonesRawString.StartsWith(PatternL)
                ? "^L.*[^R]$|^Left|^L.*"
                : "^R.*[^L]$|^Right|^R.*";
            if (split)
            {
                var splitBoneName = SplitBoneNameByPrefix(humanBodyBones, sidePrefixPattern);
                var positionPrefix = splitBoneName[0];
                var bonePartName = splitBoneName[1];

                return boneName =>
                {
                    var boneNameLr = boneName.Replace(positionPrefix, "").Replace(bonePartName, "");
                    var regex1 = new Regex(positionPrefix, RegexOptions.IgnoreCase);
                    var regex2 = new Regex(bonePartName, RegexOptions.IgnoreCase);
                    var regex3 = new Regex(sidePattern, RegexOptions.IgnoreCase);
                    return regex1.IsMatch(boneName)
                        && regex2.IsMatch(boneName)
                        && regex3.IsMatch(boneNameLr);
                };
            }

            var boneNameMatchPattern = Regex.Split(
                input: humanBodyBonesRawString,
                sidePrefixPattern
            )[1];
            return boneName =>
            {
                var regex1 = new Regex(boneNameMatchPattern, RegexOptions.IgnoreCase);
                var boneNameLr = regex1.Replace(boneName, "");
                var regex2 = new Regex(sidePattern, RegexOptions.IgnoreCase);
                return regex1.IsMatch(boneName) && regex2.IsMatch(boneNameLr);
            };
        }

        private static string[] SplitBoneNameByPrefix(
            HumanBodyBones humanBodyBones,
            string sidePrefix
        )
        {
            var rawBoneName = humanBodyBones.ToString();
            var normalizedBoneName = Regex.Split(input: rawBoneName, sidePrefix)[1];

            string prefixSplitPattern = @"([A-Z][a-z]*)";
            var prefixSplit = new Regex(prefixSplitPattern);
            return new[]
            {
                prefixSplit.Matches(normalizedBoneName)[0].Value,
                prefixSplit.Matches(normalizedBoneName)[1].Value,
            };
        }

        private static (string fingerName, string fingerPositionName) GetFingerName(
            HumanBodyBones humanBodyBones
        )
        {
            var fingerName = "";
            var fingerPositionName = "";
            switch (humanBodyBones)
            {
                case HumanBodyBones.LeftThumbProximal:
                case HumanBodyBones.LeftThumbIntermediate:
                case HumanBodyBones.LeftThumbDistal:
                case HumanBodyBones.RightThumbProximal:
                case HumanBodyBones.RightThumbIntermediate:
                case HumanBodyBones.RightThumbDistal:
                    fingerName = "Thumb";
                    break;
                case HumanBodyBones.LeftIndexProximal:
                case HumanBodyBones.LeftIndexIntermediate:
                case HumanBodyBones.LeftIndexDistal:
                case HumanBodyBones.RightIndexProximal:
                case HumanBodyBones.RightIndexIntermediate:
                case HumanBodyBones.RightIndexDistal:
                    fingerName = "Index";
                    break;
                case HumanBodyBones.LeftMiddleProximal:
                case HumanBodyBones.LeftMiddleIntermediate:
                case HumanBodyBones.LeftMiddleDistal:
                case HumanBodyBones.RightMiddleProximal:
                case HumanBodyBones.RightMiddleIntermediate:
                case HumanBodyBones.RightMiddleDistal:
                    fingerName = "Middle";
                    break;
                case HumanBodyBones.LeftRingProximal:
                case HumanBodyBones.LeftRingIntermediate:
                case HumanBodyBones.LeftRingDistal:
                case HumanBodyBones.RightRingProximal:
                case HumanBodyBones.RightRingIntermediate:
                case HumanBodyBones.RightRingDistal:
                    fingerName = "Ring";
                    break;
                case HumanBodyBones.LeftLittleProximal:
                case HumanBodyBones.LeftLittleIntermediate:
                case HumanBodyBones.LeftLittleDistal:
                case HumanBodyBones.RightLittleProximal:
                case HumanBodyBones.RightLittleIntermediate:
                case HumanBodyBones.RightLittleDistal:
                    fingerName = "Little";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(humanBodyBones),
                        humanBodyBones,
                        null
                    );
            }

            switch (humanBodyBones)
            {
                case HumanBodyBones.LeftThumbProximal:
                case HumanBodyBones.LeftIndexProximal:
                case HumanBodyBones.LeftMiddleProximal:
                case HumanBodyBones.LeftRingProximal:
                case HumanBodyBones.LeftLittleProximal:
                case HumanBodyBones.RightThumbProximal:
                case HumanBodyBones.RightIndexProximal:
                case HumanBodyBones.RightMiddleProximal:
                case HumanBodyBones.RightRingProximal:
                case HumanBodyBones.RightLittleProximal:
                    fingerPositionName = "Proximal";
                    break;
                case HumanBodyBones.LeftThumbIntermediate:
                case HumanBodyBones.LeftIndexIntermediate:
                case HumanBodyBones.LeftMiddleIntermediate:
                case HumanBodyBones.LeftRingIntermediate:
                case HumanBodyBones.LeftLittleIntermediate:
                case HumanBodyBones.RightThumbIntermediate:
                case HumanBodyBones.RightIndexIntermediate:
                case HumanBodyBones.RightMiddleIntermediate:
                case HumanBodyBones.RightRingIntermediate:
                case HumanBodyBones.RightLittleIntermediate:
                    fingerPositionName = "Intermediate";
                    break;
                case HumanBodyBones.LeftIndexDistal:
                case HumanBodyBones.LeftThumbDistal:
                case HumanBodyBones.LeftMiddleDistal:
                case HumanBodyBones.LeftRingDistal:
                case HumanBodyBones.LeftLittleDistal:
                case HumanBodyBones.RightIndexDistal:
                case HumanBodyBones.RightThumbDistal:
                case HumanBodyBones.RightMiddleDistal:
                case HumanBodyBones.RightRingDistal:
                case HumanBodyBones.RightLittleDistal:
                    fingerPositionName = "Distal";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(humanBodyBones),
                        humanBodyBones,
                        null
                    );
            }

            return (fingerName, fingerPositionName);
        }
    }
}
