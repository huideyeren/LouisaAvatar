using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XWear.IO;

namespace XWear.XWearPackage.Editor.Util.ExtraDrawer
{
    [Serializable]
    public class BodyPartGroupDetailDrawer
    {
        public HumanBodyBones[] groupedHumanBodyBones;
        public BodyPartGroupDrawer.PartGroupType partGroupType;
        public ZoomIconDrawer zoomIconDrawer;
        public bool isShow;
        public Vector2 boneListScroll;

        public void Draw(int maxHeight, int maxWidth, HumanoidMapComponent humanoidMapComponent)
        {
            if (!isShow)
            {
                return;
            }

            if (humanoidMapComponent == null)
            {
                return;
            }

            switch (partGroupType)
            {
                case BodyPartGroupDrawer.PartGroupType.Head:
                case BodyPartGroupDrawer.PartGroupType.LeftHand:
                case BodyPartGroupDrawer.PartGroupType.RightHand:
                case BodyPartGroupDrawer.PartGroupType.Torso:
                case BodyPartGroupDrawer.PartGroupType.LeftArm:
                case BodyPartGroupDrawer.PartGroupType.RightArm:
                case BodyPartGroupDrawer.PartGroupType.LeftLeg:
                case BodyPartGroupDrawer.PartGroupType.RightLeg:
                    DrawBoneList(maxHeight, humanoidMapComponent);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(partGroupType),
                        partGroupType,
                        null
                    );
            }
        }

        private void DrawBoneList(int maxHeight, HumanoidMapComponent humanoidMapComponent)
        {
            var silhouetteContent = BodyPartGroupGuiInfo.BodySilhouetteContent;
            using (
                var scroll = new EditorGUILayout.ScrollViewScope(
                    boneListScroll,
                    GUILayout.MaxHeight(maxHeight)
                )
            )
            {
                var humanoidBones = humanoidMapComponent.HumanoidMap.humanoidBones;
                foreach (var groupedHumanBodyBone in groupedHumanBodyBones)
                {
                    var humanoidBone = humanoidBones.FirstOrDefault(
                        x => x.humanBodyBones == groupedHumanBodyBone
                    );
                    if (humanoidBone == null)
                    {
                        if (GUILayout.Button($"Add not contains bone:{groupedHumanBodyBone}"))
                        {
                            humanoidBone = new HumanoidBone()
                            {
                                humanBodyBones = groupedHumanBodyBone
                            };
                            humanoidMapComponent.HumanoidMap.AddHumanoidBone(humanoidBone);
                            EditorWindow.GetWindow<XWearPackageEditorWindow>().ForceUpdate();
                            return;
                        }
                    }

                    if (humanoidBone != null)
                    {
                        DrawHumanoidMapContainsField(humanoidMapComponent, humanoidBone);
                    }

                    boneListScroll = scroll.scrollPosition;
                }
            }
        }

        private void DrawHumanoidMapContainsField(
            HumanoidMapComponent humanoidMapComponent,
            HumanoidBone humanoidBone
        )
        {
            EditorGUILayout.LabelField(humanoidBone.humanBodyBones.ToString());
            var newBone = (Transform)
                EditorGUILayout.ObjectField("", humanoidBone.bone, typeof(Transform), true);

            if (!ReferenceEquals(newBone, humanoidBone.bone))
            {
                if (
                    !humanoidMapComponent.HumanoidMap.humanoidBones
                        .Select(x => x.bone)
                        .Contains(newBone)
                )
                {
                    humanoidBone.bone = newBone;
                    EditorWindow.GetWindow<XWearPackageEditorWindow>().ForceUpdate();
                }
            }
        }
    }
}
