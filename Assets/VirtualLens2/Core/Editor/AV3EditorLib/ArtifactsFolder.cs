using System;
using System.IO;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Audio;

namespace VirtualLens2.AV3EditorLib
{
    /// <summary>
    /// Utilities for folders that contain automatically-generated assets.
    /// </summary>
    public class ArtifactsFolder
    {
        /// <summary>
        /// Initializes with an empty path to skip saving assets.
        /// </summary>
        public ArtifactsFolder()
        {
            Path = null;
        }
        
        /// <summary>
        /// Initializes with a path to the specific folder.
        /// </summary>
        /// <param name="path">The path to the folder to save assets.</param>
        public ArtifactsFolder(string path)
        {
            Path = path;
        }

        /// <summary>
        /// Creates a folder with a random name as a sibling of the marker asset.
        /// </summary>
        /// <param name="guid">GUID for the marker asset.</param>
        /// <returns>The instance of <c>ArtifactsFolder</c> that points to the created folder.</returns>
        /// <exception cref="FileNotFoundException">Marker asset was not found.</exception>
        public static ArtifactsFolder FromEmptyFile(string guid)
        {
            var empty = AssetDatabase.GUIDToAssetPath(guid);
            if (empty == null)
            {
                throw new FileNotFoundException($"Empty file ({guid}) is not found.");
            }
            var parent = System.IO.Path.GetDirectoryName(empty)?.Replace('\\', '/');
            var name = Guid.NewGuid().ToString();
            AssetDatabase.CreateFolder(parent, name);
            return new ArtifactsFolder(parent + "/" + name);
        }
        
        /// <summary>
        /// Returns the path to this folder.
        /// </summary>
        [CanBeNull]
        public string Path { get; }

        /// <summary>
        /// Removes all assets from this folder immediately.
        /// </summary>
        public void Clear()
        {
            if (Path == null) { return; }
            if (!AssetDatabase.IsValidFolder(Path)) { return; }
            AssetDatabase.DeleteAsset(Path);
        }

        private void CreateFolders(string path)
        {
            if (Path == null) { return; }
            if (AssetDatabase.IsValidFolder(path)) { return; }
            var parent = System.IO.Path.GetDirectoryName(path);
            var child = System.IO.Path.GetFileName(path);
            CreateFolders(parent);
            AssetDatabase.CreateFolder(parent, child);
        }

        private static string TypeToExtension(Type type)
        {
            if (type == typeof(AudioMixer)) { return "mixer"; }
            if (type == typeof(Material)) { return "mat"; }
            if (type == typeof(LensFlare)) { return "flare"; }
            if (type == typeof(RenderTexture)) { return "renderTexture"; }
            if (type == typeof(LightmapParameters)) { return "giparams"; }
            if (type == typeof(AnimatorController)) { return "controller"; }
            if (type == typeof(AnimationClip)) { return "anim"; }
            if (type == typeof(AnimatorOverrideController)) { return "overrideController"; }
            if (type == typeof(AvatarMask)) { return "mask"; }
            return "asset";
        }

        public string GenerateAssetPath<T>() where T : UnityEngine.Object
        {
            if (Path == null)
            {
                throw new DirectoryNotFoundException("Artifacts folder is not specified.");
            }
            return Path + "/" + Guid.NewGuid() + "." + TypeToExtension(typeof(T));
        }

        /// <summary>
        /// Saves the object as an asset with random name.
        /// </summary>
        /// <param name="obj">The object to be saved.</param>
        /// <typeparam name="T">The type of the object will be saved.</typeparam>
        public void CreateAsset<T>(T obj) where T : UnityEngine.Object
        {
            if (Path == null) { return; }
            Undo.RegisterCreatedObjectUndo(obj, "Create asset");
            CreateFolders(Path);
            var name = Guid.NewGuid() + "." + TypeToExtension(typeof(T));
            AssetDatabase.CreateAsset(obj, Path + "/" + name);
            AssetDatabase.Refresh();
        }
    }

}