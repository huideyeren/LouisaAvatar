using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XWear.IO;
using XWear.IO.XResource.Humanoid;
using XWear.XWearPackage.Editor.Validator.Error;

namespace XWear.XWearPackage.Editor.Validator.Common
{
    public class HumanoidMapComponentValidator : IXResourceValidator
    {
        private readonly Transform[] _transforms;
        private readonly GameObject _rootGameObject;

        public HumanoidMapComponentValidator(GameObject rootGameObject)
        {
            _rootGameObject = rootGameObject;
            _transforms = _rootGameObject.GetComponentsInChildren<Transform>();
        }

        public List<ValidateResult> Check()
        {
            var results = new List<ValidateResult>();

            var humanoidMap = _rootGameObject.GetComponent<HumanoidMapComponent>();

            if (humanoidMap != null)
            {
                // HumanoidMapがアタッチされているとき、設定されているBoneが正しいかをみにいく
                results.Add(CheckHumanoidMap(humanoidMap));
            }
            else
            {
                // HumanoidComponentがアタッチされていない場合、エラー
                // 自動アサインのUIを表示する
                results.Add(
                    new DefaultValidateResultError(
                        ValidateResultType.WearErrorNotFoundHumanoidComponent,
                        _rootGameObject,
                        fixActions: new FixFunctionBase[]
                        {
                            new SimpleFixFunction(
                                AddHumanoidMapComponentAndAutoAssignFromMatchedPreset,
                                FixFunctionType.AutoAssignHumanoidMapComponent
                            ),
                        }
                    )
                );
            }

            return results;
        }

        private ValidateResult CheckHumanoidMap(HumanoidMapComponent humanoidMapComponent)
        {
            // HumanoidMapの中身が空
            if (humanoidMapComponent.HumanoidMap.GetMap.Count == 0)
            {
                return new DefaultValidateResultError(
                    ValidateResultType.WearErrorEmptyHumanoidMap,
                    _rootGameObject,
                    fixActions: new FixFunctionBase[]
                    {
                        new SimpleFixFunction(
                            AddHumanoidMapComponentAndAutoAssignFromMatchedPreset,
                            FixFunctionType.AutoAssignHumanoidMapComponent
                        ),
                    }
                );
            }

            // Nullなボーンを含む
            if (humanoidMapComponent.HumanoidMap.humanoidBones.Any(x => x.bone == null))
            {
                return new DefaultValidateResultError(
                    ValidateResultType.WearErrorContainsNullHumanoidMapComponent,
                    _rootGameObject,
                    fixActions: new FixFunctionBase[]
                    {
                        new SimpleFixFunction(
                            RemoveNullTransformFromHumanoidMapComponent,
                            FixFunctionType.RemoveNullBoneFromHumanoidMapComponent
                        ),
                    }
                );
            }

            foreach (var mapKvp in humanoidMapComponent.HumanoidMap.GetMap)
            {
                var mapBone = mapKvp.Key;
                if (!_transforms.Contains(mapBone))
                {
                    // HumanoidMapにRootGameObjectの子にないTransformが含まれる
                    return new SelectableValidateResultError(
                        ValidateResultType.WearErrorInvalidHumanoidComponent,
                        _rootGameObject
                    );
                }
            }

            return new ValidateResultOk(ValidateResultType.Ok);
        }

        private Object[] RemoveNullTransformFromHumanoidMapComponent()
        {
            var humanoidMapComponent = _rootGameObject.GetComponent<HumanoidMapComponent>();

            humanoidMapComponent.HumanoidMap.humanoidBones =
                humanoidMapComponent.HumanoidMap.humanoidBones.Where(x => x.bone != null).ToList();

            return new Object[] { _rootGameObject };
        }

        private Object[] AddHumanoidMapComponentAndAutoAssignFromMatchedPreset()
        {
            var humanoidMapComponent = _rootGameObject.GetComponent<HumanoidMapComponent>();
            if (humanoidMapComponent == null)
            {
                humanoidMapComponent = _rootGameObject.AddComponent<HumanoidMapComponent>();
            }

            var resultHumanoidMap = new HumanoidMap();
            var humanoidBonePattern = GenerateHumanoidBonePatternsDictionary();
            var transforms = _rootGameObject.GetComponentsInChildren<Transform>();
            foreach (var transform in transforms)
            {
                var transformNameLower = transform.gameObject.name.ToLower();
                foreach (var humanoidBonePatternKvp in humanoidBonePattern)
                {
                    var humanBodyBones = humanoidBonePatternKvp.Key;
                    if (
                        resultHumanoidMap.humanoidBones.FirstOrDefault(
                            x => x.humanBodyBones == humanBodyBones
                        ) != null
                    )
                    {
                        continue;
                    }

                    var nameSet = humanoidBonePatternKvp.Value;
                    if (nameSet.Contains(transformNameLower))
                    {
                        resultHumanoidMap.AddHumanoidBone(
                            new HumanoidBone() { bone = transform, humanBodyBones = humanBodyBones }
                        );
                        break;
                    }
                }
            }

            // 一致したパターンが一つもなければ文字列による推定を実行する
            if (resultHumanoidMap.humanoidBones.Count == 0)
            {
                humanoidMapComponent.AutoAssign();
            }
            else
            {
                humanoidMapComponent.LoadHumanoidMap(resultHumanoidMap);
            }

            return new Object[] { _rootGameObject };
        }

        private Dictionary<HumanBodyBones, List<string>> GenerateHumanoidBonePatternsDictionary()
        {
            var presets = HumanoidMapPreset.LoadPresets();
            var result = new Dictionary<HumanBodyBones, List<string>>();
            foreach (var preset in presets)
            {
                foreach (var define in preset.humanoidBoneDefines)
                {
                    var bone = define.humanBodyBones;
                    var name = define.name.ToLower();
                    if (!result.TryGetValue(bone, out var nameSet))
                    {
                        nameSet = new List<string>();
                        result.Add(bone, nameSet);
                    }

                    nameSet.Add(name);
                }
            }

            return result;
        }
    }
}
