using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace VirtualLens2.AV3EditorLib
{

    public class VRCExpressionsMenuTemplateEngine
    {
        private List<Func<VRCExpressionsMenu.Control, VRCExpressionsMenu.Control>> _transformers =
            new List<Func<VRCExpressionsMenu.Control, VRCExpressionsMenu.Control>>();

        public void AddTransformer(Func<VRCExpressionsMenu.Control, VRCExpressionsMenu.Control> transformer)
        {
            _transformers.Add(transformer);
        }

        public VRCExpressionsMenu Apply(VRCExpressionsMenu template, ArtifactsFolder folder)
        {
            if (template == null) { return null; }
            
            VRCExpressionsMenu.Control.Parameter Duplicate(VRCExpressionsMenu.Control.Parameter p)
            {
                return new VRCExpressionsMenu.Control.Parameter {name = p.name};
            }
            
            VRCExpressionsMenu result = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            foreach (var src in template.controls)
            {
                var dst = new VRCExpressionsMenu.Control
                {
                    icon = src.icon,
                    labels = src.labels,
                    name = src.name,
                    parameter = Duplicate(src.parameter),
                    style = src.style,
                    subMenu = src.subMenu,
                    subParameters = src.subParameters.Select(Duplicate).ToArray(),
                    type = src.type,
                    value = src.value
                };
                if (dst.type == VRCExpressionsMenu.Control.ControlType.SubMenu)
                {
                    dst.subMenu = Apply(src.subMenu, folder);
                }
                foreach (var transform in _transformers)
                {
                    if (dst == null) { break; }
                    dst = transform(dst);
                }
                if (dst != null)
                {
                    result.controls.Add(dst);
                }
            }
            folder.CreateAsset(result);
            return result;
        }
    }

}