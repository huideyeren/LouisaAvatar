using System.Collections.Generic;
using UnityEngine;
using XWear.IO;
using Object = UnityEngine.Object;

namespace XWear.XWearPackage.Editor.Validator.Accessory
{
    public class RootObjectValidator : IXWearAccessoryValidator
    {
        private readonly GameObject _rootGameObject;

        public RootObjectValidator(GameObject rootGameObject)
        {
            _rootGameObject = rootGameObject;
        }

        public List<ValidateResult> Check()
        {
            var result = new List<ValidateResult>();
            result.Add(CheckAccessoryMap());

            return result;
        }

        private ValidateResult CheckAccessoryMap()
        {
            return new ValidateResultOk(ValidateResultType.AccessoryIsNotSupported);

            /*var accessoryMapComponent = _rootGameObject.GetComponent<AccessoryMapComponent>();
            if (accessoryMapComponent == null)
            {
                return new DefaultValidateResultWarning(
                    ValidateResultType.AccessoryWarningNotFoundAccessoryMap,
                    source: _rootGameObject,
                    fixActions: new FixFunctionBase[]
                    {
                        new SimpleFixFunction(
                            AddAccessoryMapComponent,
                            FixFunctionType.AddAccessoryMapComponent
                        ),
                    }
                );
            }
            else
            {
                var currentMap = accessoryMapComponent.AccessoryMap;
                if (
                    currentMap.recommendHumanBodyBones == null
                    || currentMap.recommendHumanBodyBones.Length == 0
                )
                {
                    return new SelectableValidateResultWarning(
                        ValidateResultType.AccessoryWarningMapBoneIsEmpty,
                        source: _rootGameObject
                    );
                }
            }

            return new ValidateResultOk(ValidateResultType.Ok);*/
        }

        private Object[] AddAccessoryMapComponent()
        {
            _rootGameObject.AddComponent<AccessoryMapComponent>();
            return new Object[] { _rootGameObject };
        }
    }
}
