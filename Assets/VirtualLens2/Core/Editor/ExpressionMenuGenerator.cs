using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using VirtualLens2.AV3EditorLib;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Avatars.Components;

#if WITH_MODULAR_AVATAR
using nadena.dev.modular_avatar.core;
#endif

namespace VirtualLens2
{
    internal static class ExpressionMenuGenerator
    {
        private const string ParameterPrefix = Constants.ParameterPrefix;

        private static Texture2D LoadIcon(string guid) { return AssetUtility.LoadAssetByGUID<Texture2D>(guid); }

        private static VRCExpressionsMenu GetOrCreateRootMenu(
            ImplementationSettings settings, ArtifactsFolder folder)
        {
            var avatar = settings.Avatar.GetComponent<VRCAvatarDescriptor>();
            if (avatar.expressionsMenu) { return avatar.expressionsMenu; }

            var menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            folder.CreateAsset(menu);
            avatar.expressionsMenu = menu;
            return avatar.expressionsMenu;
        }

        private static bool IsRadialPuppet(VRCExpressionsMenu.Control control, string name)
        {
            if (control.type != VRCExpressionsMenu.Control.ControlType.RadialPuppet) { return false; }
            if (control.subParameters.Length < 1) { return false; }
            return control.subParameters[0].name == name;
        }

        private static VRCExpressionsMenu GenerateMainMenu(ImplementationSettings settings, ArtifactsFolder folder)
        {
            var generator = new VRCExpressionsMenuTemplateEngine();
            if (!settings.ApertureEnabled)
            {
                generator.AddTransformer(
                    control => IsRadialPuppet(control, ParameterPrefix + "Aperture") ? null : control);
                generator.AddTransformer(control => control.name == "Focus Control" ? null : control);
            }
            if (!settings.ApertureEnabled || !settings.ManualFocusingEnabled)
            {
                generator.AddTransformer(
                    control => IsRadialPuppet(control, ParameterPrefix + "Distance") ? null : control);
            }
            if (!settings.ExposureEnabled)
            {
                generator.AddTransformer(
                    control => IsRadialPuppet(control, ParameterPrefix + "Exposure") ? null : control);
            }
            if (!settings.DroneEnabled)
            {
                generator.AddTransformer(control => control.name == "Drone" ? null : control);
            }
            // Quick calls
            generator.AddTransformer(control =>
            {
                if (control.type != VRCExpressionsMenu.Control.ControlType.SubMenu) { return control; }
                if (control.name != "Quick Calls") { return control; }
                if (control.subMenu == null || control.subMenu.controls.Count == 0) { return null; }
                return control;
            });
            generator.AddTransformer(control =>
            {
                VRCExpressionsMenu.Control.Parameter Duplicate(VRCExpressionsMenu.Control.Parameter p)
                {
                    return new VRCExpressionsMenu.Control.Parameter { name = p.name };
                }

                if (control.parameter.name != ParameterPrefix + "Control") { return control; }
                var quickCall0 = (int)MenuTrigger.QuickCall0;
                var index = (int)control.value - quickCall0;
                if (index < 0 || Constants.NumQuickCalls <= index) { return control; }
                if (index >= settings.QuickCalls.Count) { return null; }
                return new VRCExpressionsMenu.Control
                {
                    icon = control.icon,
                    labels = control.labels,
                    name = settings.QuickCalls[index].Name,
                    parameter = Duplicate(control.parameter),
                    style = control.style,
                    subMenu = control.subMenu,
                    subParameters = control.subParameters.Select(Duplicate).ToArray(),
                    type = control.type,
                    value = control.value
                };
            });
            // Custom grids
            generator.AddTransformer(control =>
            {
                VRCExpressionsMenu.Control.Parameter Duplicate(VRCExpressionsMenu.Control.Parameter p)
                {
                    return new VRCExpressionsMenu.Control.Parameter { name = p.name };
                }

                if (control.parameter.name != ParameterPrefix + "Control") { return control; }
                var customGrid0 = (int)MenuTrigger.GridCustom0;
                var index = (int)control.value - customGrid0;
                if (index < 0 || Constants.NumCustomGrids <= index) { return control; }
                if (index >= settings.CustomGrids.Count) { return null; }
                return new VRCExpressionsMenu.Control
                {
                    icon = control.icon,
                    labels = control.labels,
                    name = settings.CustomGrids[index].Name,
                    parameter = Duplicate(control.parameter),
                    style = control.style,
                    subMenu = control.subMenu,
                    subParameters = control.subParameters.Select(Duplicate).ToArray(),
                    type = control.type,
                    value = control.value,
                };
            });
            // VirtualLens2/Core/Expressions/VirtualLens2.asset
            var template = AssetUtility.LoadAssetByGUID<VRCExpressionsMenu>("aa744a75e61afb143be7425bb3b0cce9");
            return generator.Apply(template, folder);
        }

        public static void Generate(ImplementationSettings settings, GeneratedObjectSet objectSet,
            ArtifactsFolder folder)
        {
            Clear(settings);
            if (settings.RemoteOnly) { return; }
            if (settings.BuildAsModule)
            {
                UpdateModularAvatarMenuInstaller(settings, objectSet, folder);
            }
            else if (settings.MenuInstallerObject != null)
            {
                CreateMenuInstaller(settings, folder);
            }
            else
            {
                UpdateNativeMenu(settings, folder);
            }
        }

        private static void UpdateNativeMenu(ImplementationSettings settings, ArtifactsFolder folder)
        {
            var root = GetOrCreateRootMenu(settings, folder);
            Undo.RecordObject(root, "Update expressions menu");
            var menu = GenerateMainMenu(settings, folder);

            // VirtualLens2: VirtualLens2/Core/Images/vlens.png
            const string name = "VirtualLens2";
            root.controls.RemoveAll(
                e => e.name == name && e.type == VRCExpressionsMenu.Control.ControlType.SubMenu);
            var control = new VRCExpressionsMenu.Control()
            {
                name = name,
                icon = LoadIcon("1e4c5e5c72d54bb449d54e843f1a97d7"),
                type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                subMenu = menu,
                subParameters = new VRCExpressionsMenu.Control.Parameter[] { },
                labels = new VRCExpressionsMenu.Control.Label[] { }
            };
            root.controls.Add(control);
            EditorUtility.SetDirty(root);
        }

        private static VRCExpressionsMenu CreateInstallableMenu(ImplementationSettings settings, ArtifactsFolder folder)
        {
            var menu = GenerateMainMenu(settings, folder);
            var root = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            folder.CreateAsset(root);

            // VirtualLens2: VirtualLens2/Core/Images/vlens.png
            const string name = "VirtualLens2";
            var control = new VRCExpressionsMenu.Control()
            {
                name = name,
                icon = LoadIcon("1e4c5e5c72d54bb449d54e843f1a97d7"),
                type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                subMenu = menu,
                subParameters = new VRCExpressionsMenu.Control.Parameter[] { },
                labels = new VRCExpressionsMenu.Control.Label[] { }
            };
            root.controls.Add(control);
            EditorUtility.SetDirty(root);

            return root;
        }

        private static void CreateMenuInstaller(ImplementationSettings settings, ArtifactsFolder folder)
        {
#if WITH_MODULAR_AVATAR
            var marker = settings.MenuInstallerObject;

            var obj = new GameObject
            {
                name = "_VirtualLens_MenuInstaller",
                transform =
                {
                    localPosition = marker.transform.localPosition,
                    localRotation = marker.transform.localRotation,
                    localScale = marker.transform.localScale,
                    parent = marker.transform.parent,
                }
            };
            obj.transform.SetSiblingIndex(marker.transform.GetSiblingIndex());

            var root = CreateInstallableMenu(settings, folder);
            var src = marker.GetComponent<ModularAvatarMenuInstaller>();
            var dst = obj.AddComponent<ModularAvatarMenuInstaller>();
            if (src != null)
            {
                EditorUtility.CopySerialized(src, dst);
            }
            dst.menuToAppend = root;
            Undo.RegisterCreatedObjectUndo(obj, "Create MenuInstaller");
#else
            throw new ApplicationException("Modular Avatar is not installed for this project.");
#endif
        }

        private static void UpdateModularAvatarMenuInstaller(
            ImplementationSettings settings, GeneratedObjectSet objectSet, ArtifactsFolder folder)
        {
#if WITH_MODULAR_AVATAR
            var component = objectSet.VirtualLensRoot.GetComponent<ModularAvatarMenuInstaller>();
            Undo.RecordObject(component, "Update modular avatar menu installer");
            var so = new SerializedObject(component);
            var root = CreateInstallableMenu(settings, folder);
            so.FindProperty("menuToAppend").objectReferenceValue = root;
            so.ApplyModifiedProperties();
#else
            throw new ApplicationException("Modular Avatar is not installed for this project.");
#endif
        }

        public static void Clear(GameObject avatar)
        {
            if (!avatar) { return; }
            var descriptor = avatar.GetComponent<VRCAvatarDescriptor>();
            if (!descriptor || !descriptor.expressionsMenu) { return; }
            var menu = descriptor.expressionsMenu;
            Undo.RecordObject(menu, "Remove expressions menu items");
            menu.controls.RemoveAll(
                e => e.name == "VirtualLens2" && e.type == VRCExpressionsMenu.Control.ControlType.SubMenu);
            menu.controls.RemoveAll(
                e => e.name == "VirtualLens2 Quick Calls" && e.type == VRCExpressionsMenu.Control.ControlType.SubMenu);
            EditorUtility.SetDirty(menu);
        }

        private static void Clear(ImplementationSettings settings) { Clear(settings.Avatar); }
    }
}
