using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using VirtualLens2.AV3EditorLib;
using VRC.Core;
using Object = UnityEngine.Object;

namespace VirtualLens2
{
    internal static class ObjectGenerator
    {
        private static GameObject CreateTransformReference(ImplementationSettings settings)
        {
            var origin = MarkerDetector.DetectOrigin(settings);
            var droppable = settings.CameraNonPreviewRoot;
            var parent = droppable.transform.parent;
            var reference = new GameObject
            {
                name = "_VirtualLens_TransformReference",
                transform =
                {
                    parent = parent,
                    position = droppable.transform.position,
                    rotation = origin.transform.rotation
                }
            };
            return reference;
        }

        private static GameObject CreateSelfieMarkers(ImplementationSettings settings)
        {
            var animator = settings.Avatar.GetComponent<Animator>();
            if (!animator) { return null; }

            // Get related transforms
            var head = animator.GetBoneTransform(HumanBodyBones.Head);
            var leftEye = animator.GetBoneTransform(HumanBodyBones.LeftEye);
            var rightEye = animator.GetBoneTransform(HumanBodyBones.RightEye);
            if (head == null || leftEye == null || rightEye == null) { return null; }
            var leftMarkerObj = settings.SelfieMarkerLeft;
            var rightMarkerObj = settings.SelfieMarkerRight;
            if (leftMarkerObj == null || rightMarkerObj == null) { return null; }
            var leftMarker = leftMarkerObj.transform;
            var rightMarker = rightMarkerObj.transform;

            // VirtualLens2/Core/Prefabs/AutoFocus/SelfieFocusDetector.prefab
            var prefab = AssetUtility.LoadAssetByGUID<GameObject>("50c3bac1529a2e24cac4df738044650c");
            var root = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (root == null)
            {
                throw new ApplicationException(
                    "Failed to instantiate SelfieFocusDetector prefab.\n" +
                    "Please reimport VirtualLens2 package.");
            }
            root.name = "_VirtualLens_SelfieFocusDetector";

            // Update transforms
            root.transform.parent = head.parent;
            root.transform.position = head.position;
            root.transform.rotation = head.rotation;
            root.transform.localScale = Vector3.one;
            root.GetComponent<RotationConstraint>().AddSource(
                new ConstraintSource() {sourceTransform = head, weight = 1.0f});
            // Left eye
            var copyLeftEye = HierarchyUtil.PathToTransform(root.transform, "LeftEye");
            copyLeftEye.position = leftEye.position;
            copyLeftEye.rotation = leftEye.rotation;
            copyLeftEye.GetComponent<RotationConstraint>().SetSource(
                0, new ConstraintSource {sourceTransform = leftEye, weight = 1.0f});
            // Right eye
            var copyRightEye = HierarchyUtil.PathToTransform(root.transform, "RightEye");
            copyRightEye.position = rightEye.position;
            copyRightEye.rotation = rightEye.rotation;
            copyRightEye.GetComponent<RotationConstraint>().SetSource(
                0, new ConstraintSource {sourceTransform = rightEye, weight = 1.0f});
            // Left marker
            var copyLeftMarker = HierarchyUtil.PathToTransform(copyLeftEye, "Detector");
            copyLeftMarker.position = leftMarker.position;
            copyLeftMarker.rotation = leftMarker.rotation;
            // Right marker
            var copyRightMarker = HierarchyUtil.PathToTransform(copyRightEye, "Detector");
            copyRightMarker.position = rightMarker.position;
            copyRightMarker.rotation = rightMarker.rotation;

            root.SetActive(false);
            return root;
        }

        private static void ConfigureNonPreviewConstraint(
            ImplementationSettings settings, GameObject root, GameObject transformReference)
        {
            var source = HierarchyUtility.PathToObject(root, "Local/Transform/Result");
            var target = settings.CameraNonPreviewRoot;
            var constraint = target.AddComponent<ParentConstraint>();
            constraint.constraintActive = false;
            constraint.weight = 0.0f;
            constraint.AddSource(new ConstraintSource {sourceTransform = source.transform, weight = 1.0f});
            var q1 = transformReference.transform.rotation;
            var q2 = target.transform.rotation;
            constraint.SetTranslationOffset(0, Vector3.zero);
            constraint.SetRotationOffset(0, (Quaternion.Inverse(q1) * q2).eulerAngles);
            constraint.locked = true;
            constraint.constraintActive = true;
        }

        private static void ConfigureTouchableCamera(ImplementationSettings settings, GameObject root)
        {
            var parent = MarkerDetector.DetectPreview(settings);
            var scale = parent.transform.lossyScale.y;
            var camera = HierarchyUtility
                .PathToObject(root, "Local/Preview/Camera")
                .GetComponent<Camera>();
            camera.orthographicSize = scale * 0.5f * (16.0f / 9.0f);
            camera.nearClipPlane = scale * 0.02f;
            camera.farClipPlane = scale * 0.2f * settings.TouchScreenThickness;
        }
        
        [SuppressMessage("ReSharper", "Unity.PreferAddressByIdToGraphicsParams")]
        private static void ConfigureComputeEngine(
            ImplementationSettings settings, ArtifactsFolder folder, GameObject root)
        {
            void SetMultiSamplingKeyword(Material material)
            {
                if (settings.MSAASamples == 0)
                {
                    material.DisableKeyword("WITH_MULTI_SAMPLING");
                }
                else
                {
                    material.EnableKeyword("WITH_MULTI_SAMPLING");
                }
            }
            
            var cameraRoot = HierarchyUtility.PathToObject(root, "Local/Capture/Camera");
            var computesRoot = HierarchyUtility.PathToObject(root, "Local/Compute");
            var writersRoot = HierarchyUtility.PathToObject(root, "Local/Writer");
            RenderTexture rgbTexture, depthTexture;
            
            if (!settings.UseHighResolution)
            {
                var antialias = (int) settings.AntialiasingMode;
                var msaa = settings.MSAASamples;
                
                // VirtualLens2/Core/Texture/LogiBokeh/Realtime/RGB_*.renderTexture
                var rgbTextures = new[]
                {
                    "c6e04e01e7982dd488094c5b1e2ac4b7", // RGB_1x.renderTexture
                    "a4ea57255a63fd444acbbe201efa416b", // RGB_2x.renderTexture
                    "2f79c55b95dd71246a8c2ad42cf8bb00", // RGB_4x.renderTexture
                    "2980f1542f75232489fba59bb83a8bbb", // RGB_8x.renderTexture
                };
                var rgbCameras = HierarchyUtility
                    .PathToObject(cameraRoot, "RGB")
                    .GetComponentsInChildren<Camera>(true);
                rgbTexture = AssetUtility.LoadAssetByGUID<RenderTexture>(rgbTextures[msaa]);
                foreach (var camera in rgbCameras)
                {
                    camera.targetTexture = rgbTexture;
                }

                // VirtualLens2/Core/Texture/LogiBokeh/Realtime/Depth_*.renderTexture
                var depthTextures = new[]
                {
                    "0f674a2894242f94a8629e865ab90372", // Depth_1x.renderTexture
                    "d92b6b9bd81b9fb46ad353ccff6cbafa", // Depth_2x.renderTexture
                    "8dab3d4fcc1168d4b8103304fa5e1b13", // Depth_4x.renderTexture
                    "c362fdaa7824ff444ba20a338548f6f8", // Depth_8x.renderTexture
                };
                var depthCameras = HierarchyUtility
                    .PathToObject(cameraRoot, "Depth")
                    .GetComponentsInChildren<Camera>(true);
                depthTexture = AssetUtility.LoadAssetByGUID<RenderTexture>(depthTextures[msaa]);
                foreach (var camera in depthCameras)
                {
                    camera.targetTexture = depthTexture;
                }

                var computeRoot = HierarchyUtility.PathToObject(computesRoot, "Normal");
                computeRoot.tag = "Untagged";
                computeRoot.SetActive(true);
                
                // VirtualLens2/Core/Materials/LogiBokeh/Realtime/ComputeCoc_*.mat
                var computeCocMaterials = new[]
                {
                    "8f8c4fa9e8866a6469548b1934d6bb4b",  // ComputeCoc_1x.mat
                    "2cda74f8285df4743b27f61a8d66b40f",  // ComputeCoc_2x.mat
                    "11e2966b12ceb41479d98ef36740ba74",  // ComputeCoc_4x.mat
                    "d386f215bc06a4e4fb75afe2f26b4182",  // ComputeCoc_8x.mat
                };
                var computeCoc = HierarchyUtil
                    .PathToObject(computeRoot, "ComputeCoc/Quad")
                    .GetComponent<MeshRenderer>();
                computeCoc.material = AssetUtility.LoadAssetByGUID<Material>(computeCocMaterials[msaa]);
                
                // VirtualLens2/Core/Materials/LogiBokeh/Realtime/Compute*.mat
                var computeMaterials = new[]
                {
                    new[]
                    {
                        "db2ffd10bdc974642a61d2621ec3b172", // Compute_1x.mat
                        "7bab98989caf97441b8d02eb48e36196", // Compute_2x.mat
                        "96ca0c0389237c44784507e01f086333", // Compute_4x.mat
                        "b12b144eaf3f5ea4e9b8faa1fd5bf71d", // Compute_8x.mat
                    },
                    new[]
                    {
                        "6be78ec1e28878e49a640be53efd2434", // ComputeFXAA_1x.mat
                        "16e50fc22c1014d44bb94801a3926883", // ComputeFXAA_2x.mat
                        "6a01c975834cc344e95c8bcfbdb47cb2", // ComputeFXAA_4x.mat
                        "febf570cc2b202b46967e33e7e73854a", // ComputeFXAA_8x.mat
                    },
                    new[]
                    {
                        "7841e1bf30bb71f4a8753949d444544c", // ComputeSMAA_1x.mat
                        "aeda956d94ba95c4b91fda0f5edaa763", // ComputeSMAA_2x.mat
                        "e309d883585759c439e896b45cf20c40", // ComputeSMAA_4x.mat
                        "9f8d0afe6f18d404492916ef68040ec5", // ComputeSMAA_8x.mat
                    },
                };
                var compute = HierarchyUtil
                    .PathToObject(computeRoot, "Compute/Quad")
                    .GetComponent<MeshRenderer>();
                compute.material = AssetUtility.LoadAssetByGUID<Material>(computeMaterials[antialias][msaa]);
                
                var writer = HierarchyUtility.PathToObject(writersRoot, "Normal");
                writer.tag = "Untagged";
                writer.SetActive(true);
            }
            else
            {
                var antialias = (int) settings.AntialiasingMode;
                var realtimeAntialias = (int) settings.RealtimeAntialiasingMode;
                var msaa = settings.MSAASamples;
                
                // VirtualLens2/Core/Texture/LogiBokeh/HighResolution/RGB_*.renderTexture
                var rgbTextures = new[]
                {
                    "25a6cfd1de6a8c142b1b3fa9bb2fb2e6", // RGB_1x.renderTexture
                    "affaca4e95d4a5c45bf6c0c5c7aad57c", // RGB_2x.renderTexture
                    "d3f453510d968214fa5230567a1c7f37", // RGB_4x.renderTexture
                    "618e2556430ee7043a4fadfb29007d8d", // RGB_8x.renderTexture
                };
                var rgbCameras = HierarchyUtility
                    .PathToObject(cameraRoot, "RGB")
                    .GetComponentsInChildren<Camera>(true);
                rgbTexture = AssetUtility.LoadAssetByGUID<RenderTexture>(rgbTextures[msaa]);
                foreach (var camera in rgbCameras)
                {
                    camera.targetTexture = rgbTexture;
                }

                // VirtualLens2/Core/Texture/LogiBokeh/HighResolution/Depth_*.renderTexture
                var depthTextures = new[]
                {
                    "9b14ab2c7d8ea7b4ca42056e1391608c", // Depth_1x.renderTexture
                    "faa73bd9f5d8be84f9181ebe1f7e31a9", // Depth_2x.renderTexture
                    "e04eada65fb4a50488c2d1f014fe26b4", // Depth_4x.renderTexture
                    "273dfdf41bea25140b583ea029e47fb1", // Depth_8x.renderTexture
                };
                var depthCameras = HierarchyUtility
                    .PathToObject(cameraRoot, "Depth")
                    .GetComponentsInChildren<Camera>(true);
                depthTexture = AssetUtility.LoadAssetByGUID<RenderTexture>(depthTextures[msaa]);
                foreach (var camera in depthCameras)
                {
                    camera.targetTexture = depthTexture;
                }

                var computeRoot = HierarchyUtility.PathToObject(computesRoot, "HighResolution");
                computeRoot.tag = "Untagged";
                computeRoot.SetActive(true);
                
                // VirtualLens2/Core/Materials/LogiBokeh/HighResolution/ComputeCoc_*.mat
                var computeCocMaterials = new[]
                {
                    "48cf17ad886faf84b99410655e00dad9",  // ComputeCoc_1x.mat
                    "6106282f19d8d8d46bf8d63e2c44b9ca",  // ComputeCoc_2x.mat
                    "00db84df0c392804493100013f30f807",  // ComputeCoc_4x.mat
                    "f24329d7862552b4095c92dc2f058f33",  // ComputeCoc_8x.mat
                };
                var computeCoc = HierarchyUtil
                    .PathToObject(computeRoot, "ComputeCoc/Quad")
                    .GetComponent<MeshRenderer>();
                computeCoc.material = AssetUtility.LoadAssetByGUID<Material>(computeCocMaterials[msaa]);
                
                // VirtualLens2/Core/Materials/LogiBokeh/HighResolution/Downsample_*.mat
                var downsampleMaterials = new[]
                {
                    "2927bfa041e43084f895e3621c65cdff",  // Downsample_1x.mat
                    "d0a018925d23cea4286a07ef9391897a",  // Downsample_2x.mat
                    "64cc4ed4e895a8241bd1c4020f9fbee5",  // Downsample_4x.mat
                    "0f5789f1a5fae1440bb50d326a5a2cc0",  // Downsample_8x.mat
                };
                var downsample = HierarchyUtil
                    .PathToObject(computeRoot, "Downsample/Quad")
                    .GetComponent<MeshRenderer>();
                downsample.material = AssetUtility.LoadAssetByGUID<Material>(downsampleMaterials[msaa]);
                
                // VirtualLens2/Core/Materials/LogiBokeh/HighResolution/RealtimeCompute*.mat
                var realtimeMaterials = new[]
                {
                    "646b3ee4748b1da4c977c09422a0913c",  // RealtimeCompute.mat
                    "c589d6c6d6485b84db9709ba19daae86",  // RealtimeComputeFXAA.mat
                    "87c3d3c12484a9e498f8a2362ae5a981",  // RealtimeComputeSMAA.mat
                };
                var realtimeCompute = HierarchyUtil
                    .PathToObject(computeRoot, "RealtimeCompute/Quad")
                    .GetComponent<MeshRenderer>();
                realtimeCompute.material = AssetUtility.LoadAssetByGUID<Material>(realtimeMaterials[realtimeAntialias]);
                
                var writer = HierarchyUtility.PathToObject(writersRoot, "HighResolution");
                writer.tag = "Untagged";
                writer.SetActive(true);

                // VirtualLens2/Core/Materials/LogiBokeh/HighResolution/
                var writerMaterials = new[]
                {
                    new[]
                    {
                        "51f17b6f9a6e606459355691d498d2dc",  // HighResolutionCompute_1x.mat
                        "06586a7584456654bbea40076695658d",  // HighResolutionCompute_2x.mat
                        "1905df26c610b664daf2e534e49fa00f",  // HighResolutionCompute_4x.mat
                        "f610bd97a4507ce428a1d7af9b3b7356",  // HighResolutionCompute_8x.mat
                    },
                    new[]
                    {
                        "ca622c1c676f93848a072d20ceec97b4",  // HighResolutionComputeFXAA_1x.mat
                        "440630e638942f74096669cd0d21ba4e",  // HighResolutionComputeFXAA_2x.mat
                        "df77e97c923b8394d84b2327484c13bb",  // HighResolutionComputeFXAA_4x.mat
                        "d11b0ee20122a2947ab6cc6d856a51bd",  // HighResolutionComputeFXAA_8x.mat
                    },
                    new[]
                    {
                        "0d79d0efc53793948ad37fe2d0f0f675",  // HighResolutionComputeSMAA_1x.mat
                        "5142658629289b34e8059073a117f1f8",  // HighResolutionComputeSMAA_2x.mat
                        "9efae623e2ca09d4b80b49fb5f7f3d10",  // HighResolutionComputeSMAA_4x.mat
                        "8f9015a1a8037c04cb325971697f7dd6",  // HighResolutionComputeSMAA_8x.mat
                    },
                };
                var compute = HierarchyUtility
                    .PathToObject(writer, "HighResolutionWriter")
                    .GetComponent<MeshRenderer>();
                compute.material = AssetUtility.LoadAssetByGUID<Material>(writerMaterials[antialias][msaa]);
            }
            
            // VirtualLens2/Core/Materials/AutoFocus/FaceFocusCompute.mat
            var faceFocusMaterial = Object.Instantiate(
                AssetUtility.LoadAssetByGUID<Material>("29e488319d742f346abb335808f74999"));
            SetMultiSamplingKeyword(faceFocusMaterial);
            faceFocusMaterial.SetTexture("_InputTex", rgbTexture);
            faceFocusMaterial.SetInt("_Use_4K_Input", settings.UseHighResolution ? 1 : 0);
            folder.CreateAsset(faceFocusMaterial);
            var faceFocusCompute = HierarchyUtility
                .PathToObject(computesRoot, "FaceFocusCompute/Quad")
                .GetComponent<MeshRenderer>();
            faceFocusCompute.material = faceFocusMaterial;
            
            // VirtualLens2/Core/Materials/System/StateUpdaterWithExtensions.mat
            var stateUpdaterMaterial = Object.Instantiate(
                AssetUtility.LoadAssetByGUID<Material>("abbc98ae909d3f945bda1eb2f7b8708f"));
            SetMultiSamplingKeyword(stateUpdaterMaterial);
            stateUpdaterMaterial.SetTexture("_DepthTex", depthTexture);
            folder.CreateAsset(stateUpdaterMaterial);
            var stateUpdater = HierarchyUtility
                .PathToObject(computesRoot, "StateUpdater/Quad")
                .GetComponent<MeshRenderer>();
            stateUpdater.material = stateUpdaterMaterial;
            
            // VirtualLens2/Core/Materials/System/DisplayRenderer.mat
            var displayRendererMaterial = Object.Instantiate(
                AssetUtility.LoadAssetByGUID<Material>("f90d649c3764726458e6844c6c377319"));
            SetMultiSamplingKeyword(displayRendererMaterial);
            displayRendererMaterial.SetTexture("_DepthTex", depthTexture);
            for (var i = 0; i < settings.CustomGrids.Count; ++i)
            {
                displayRendererMaterial.SetTexture($"_CustomGrid{i}Tex", settings.CustomGrids[i].Texture);
            }
            folder.CreateAsset(displayRendererMaterial);
            var displayRenderer = HierarchyUtility
                .PathToObject(computesRoot, "DisplayRenderer/Quad")
                .GetComponent<MeshRenderer>();
            displayRenderer.material = displayRendererMaterial;
        }

        private static void ConfigureDroneSpeed(ImplementationSettings settings, GameObject root)
        {
            var s = settings.DroneLinearSpeedScale;
            var tf = HierarchyUtility
                .PathToObject(root, "Local/Transform/SpeedControl/Drone")
                .transform;
            tf.localScale = new Vector3(s, s, s);
        }

        private static void ConfigureDroneDeadZone(ImplementationSettings settings, GameObject root)
        {
            var x = settings.DroneLinearDeadZoneSize;
            var tf = HierarchyUtility
                .PathToObject(root, "Local/Transform/Controller/Enabler/Arm0/Arm1")
                .transform;
            tf.localPosition = new Vector3(x, 0.0f, 0.0f);
        }

        private static void CreateScreenToucherMeshes(ImplementationSettings settings)
        {
            // VirtualLens2/Core/Prefabs/ScreenToucherMesh.prefab
            var prefab = AssetUtility.LoadAssetByGUID<GameObject>("b9efe750bc19fe94797ce4166f15d90f");
            foreach (var container in settings.ScreenTouchers)
            {
                var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (instance == null)
                {
                    throw new ApplicationException(
                        "Failed to instantiate ScreenToucherMesh prefab.\n" +
                        "Please reimport VirtualLens2 package");
                }
                instance.name = "_VirtualLens_ScreenToucher";
                instance.transform.parent = container.transform;
                TransformUtil.ResetLocalTransform(instance.transform);
                PrefabUtility.UnpackPrefabInstance(
                    instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                Undo.RegisterCreatedObjectUndo(instance, "Create ScreenToucherMesh");
            }
        }

        private static void InsertVersionMarker(GameObject root)
        {
            // ReSharper disable once UnusedVariable
            var marker = new GameObject
            {
                name = "_Version" + Constants.Version,
                transform =
                {
                    parent = root.transform,
                    position = Vector3.zero,
                    rotation = Quaternion.identity
                }
            };
        }

        public static GeneratedObjectSet Generate(ImplementationSettings settings, ArtifactsFolder folder)
        {
            const string rootName = "_VirtualLens_Root";
            
            Clear(settings);

            if (settings.RemoteOnly)
            {
                var remoteRoot = new GameObject
                {
                    name = rootName,
                    transform = { parent = settings.Avatar.transform }
                };
                InsertVersionMarker(remoteRoot);
                return new GeneratedObjectSet
                {
                    VirtualLensRoot = remoteRoot,
                    SelfieDetectorMarkers = null
                };
            }

            var transformReference = CreateTransformReference(settings);
            var selfieMarkers = CreateSelfieMarkers(settings);

            var prefabParams = new GameObjectTemplateParameters();
            var origin = MarkerDetector.DetectOrigin(settings);
            prefabParams.Add("CaptureOrigin", origin);
            prefabParams.Add("Preview", MarkerDetector.DetectPreview(settings));
            prefabParams.Add("RepositionOrigin", settings.RepositionOrigin);
            prefabParams.Add("DroneController", settings.DroneController);
            prefabParams.Add("TransformReference", transformReference);
            prefabParams.Add(
                "ExternalSource", settings.ExternalPoseSource ? settings.ExternalPoseSource.gameObject : origin);

            var prefab = AssetUtility.LoadAssetByGUID<GameObject>(settings.BuildAsModule
                ? "7eb6041c7f2c759499bedf567239930b"   // VirtualLens2/Core/Prefabs/RootMA.prefab
                : "236ed580f6b14b044961ddd465461d2b"); // VirtualLens2/Core/Prefabs/Root.prefab
            var root = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (root == null)
            {
                throw new ApplicationException(
                    "Failed to instantiate root prefab.\n" +
                    "Please reimport VirtualLens2 package");
            }
            Undo.RegisterCreatedObjectUndo(root, "Create VirtualLensRoot");
            root.name = rootName;
            root.transform.parent = settings.Avatar.transform;
            TransformUtil.ResetLocalTransform(root.transform);
            GameObjectTemplateEngine.Apply(root, prefabParams);

            ConfigureNonPreviewConstraint(settings, root, transformReference);
            ConfigureTouchableCamera(settings, root);
            ConfigureComputeEngine(settings, folder, root);
            ConfigureDroneSpeed(settings, root);
            ConfigureDroneDeadZone(settings, root);
            CreateScreenToucherMeshes(settings);
            InsertVersionMarker(root);

            return new GeneratedObjectSet
            {
                VirtualLensRoot = root,
                SelfieDetectorMarkers = selfieMarkers
            };
        }


        private static void RemoveConstraints(GameObject obj)
        {
            if (obj == null) { return; }
            var position = obj.GetComponent<PositionConstraint>();
            if (position) { Object.DestroyImmediate(position); }
            var rotation = obj.GetComponent<RotationConstraint>();
            if (rotation) { Object.DestroyImmediate(rotation); }
            var parent = obj.GetComponent<ParentConstraint>();
            if (parent) { Object.DestroyImmediate(parent); }
        }

        private static void RemoveDescendants(GameObject obj, Regex regex)
        {
            if (obj == null) { return; }
            var candidates = new List<GameObject>();
            foreach (Transform transform in obj.transform)
            {
                var child = transform.gameObject;
                if (regex.IsMatch(child.name))
                {
                    candidates.Add(child);
                }
                else
                {
                    RemoveDescendants(child, regex);
                }
            }
            foreach (var child in candidates)
            {
                Object.DestroyImmediate(child);
            }
        }

        private static void RemoveDescendants(GameObject obj)
        {
            RemoveDescendants(obj, new Regex(@"^.*$"));
        }

        public static void Clear(GameObject avatar, GameObject cameraRoot, GameObject nonPreviewRoot)
        {
            // Remove constraints from camera objects
            RemoveConstraints(cameraRoot);
            RemoveConstraints(nonPreviewRoot);
            // Remove auto-generated objects
            RemoveDescendants(avatar, new Regex(@"^_VirtualLens_.*$"));
            RemoveDescendants(MarkerDetector.DetectOrigin(cameraRoot));
            RemoveDescendants(MarkerDetector.DetectPreview(cameraRoot));
        }

        private static void Clear(ImplementationSettings settings)
        {
            Clear(settings.Avatar, settings.CameraRoot, settings.CameraNonPreviewRoot);
        }
    }
    
}
