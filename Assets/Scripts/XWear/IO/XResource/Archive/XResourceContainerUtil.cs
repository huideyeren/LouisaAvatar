using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEngine;
using XWear.IO.Thumbnail;
using XWear.IO.XResource.Animation;
using XWear.IO.XResource.AvatarMeta;
using XWear.IO.XResource.Component;
using XWear.IO.XResource.Mesh;
using XWear.IO.XResource.Texture;

namespace XWear.IO.XResource.Archive
{
    public static class XResourceContainerUtil
    {
        private const string MetaFileName = "XItem.meta.json";
        private const string ThumbnailFileName = "Thumbnail.png";
        private const string XTextureFolderName = "Textures";
        private const string XMeshFolderName = "Mesh";

        private const string ExternalResourceFolderName = "External";

        private static string VrcAssetResourceFolderName =>
            Path.Combine(ExternalResourceFolderName, "VrcAssets");

        private const string ExpressionsMenuFolderName = "Expressions";
        private const string ExpressionsSubMenuFolderName = "SubMenu";
        private const string ExpressionsSubMenuIconFolderName = "Icons";

        private const string AnimationsFolderName = "Animations";
        private const string AnimatorControllersFolderName = "Controllers";
        private const string AvatarMasksFolderName = "AvatarMasks";

        private const string AnimationClipsFolderName = "Animations";

        private const string XItemDirName = "XItems";

        private static string GenerateXItemFileName(string guid)
        {
            return Path.Combine(XItemDirName, guid + ".json");
        }

        private static string GetTexturePath(XResourceTexture xResourceTexture)
        {
            return Path.Combine(XTextureFolderName, xResourceTexture.Guid + ".png");
        }

        private static string GetMeshPath(
            XResourceSkinnedMeshRenderer xResourceSkinnedMeshRenderer,
            bool isDefault
        )
        {
            var fileName = xResourceSkinnedMeshRenderer.Guid;
            if (isDefault)
            {
                fileName = "Default" + "-" + xResourceSkinnedMeshRenderer.Guid;
            }

            return Path.Combine(XMeshFolderName, fileName + ".bin");
        }

        private static string GetVertexActiveMarksPath(
            XResourceSkinnedMeshRenderer xResourceSkinnedMeshRenderer
        )
        {
            var fileName = "ActiveMark" + "-" + xResourceSkinnedMeshRenderer.Guid;
            return Path.Combine(XMeshFolderName, fileName);
        }

        public static string GetAnimationClipFolder()
        {
            return Path.Combine(ExternalResourceFolderName, AnimationClipsFolderName);
        }

        private static string GetAssetResourceFolder(VrcAssetResource.AssetType assetType)
        {
            var folderName = VrcAssetResourceFolderName;
            switch (assetType)
            {
                case VrcAssetResource.AssetType.AnimatorController:
                    folderName = Path.Combine(
                        folderName,
                        AnimationsFolderName,
                        AnimatorControllersFolderName
                    );
                    break;
                case VrcAssetResource.AssetType.AvatarMask:
                    folderName = Path.Combine(folderName, AvatarMasksFolderName);
                    break;
                case VrcAssetResource.AssetType.ExpressionsMainMenu:
                case VrcAssetResource.AssetType.ExpressionParameters:
                    // VrcAssets/ExpressionsMenu
                    folderName = Path.Combine(folderName, ExpressionsMenuFolderName);
                    break;
                case VrcAssetResource.AssetType.ExpressionsSubMenu:
                    // VrcAssets/ExpressionsMenu/SubMenu
                    folderName = Path.Combine(
                        folderName,
                        ExpressionsMenuFolderName,
                        ExpressionsSubMenuFolderName
                    );
                    break;
                case VrcAssetResource.AssetType.ExpressionsIcon:
                    // VrcAssets/ExpressionsMenu/Icons
                    folderName = Path.Combine(
                        folderName,
                        ExpressionsMenuFolderName,
                        ExpressionsSubMenuIconFolderName
                    );
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(assetType), assetType, null);
            }

            return folderName;
        }

        public class XResourceOpener : IDisposable
        {
            private readonly ZipArchive _zipArchive;
            private readonly ReadOnlyCollection<ZipArchiveEntry> _entries;

            public XResourceOpener(FileStream stream)
            {
                _zipArchive = new ZipArchive(stream, ZipArchiveMode.Read, true, Encoding.UTF8);
                _entries = _zipArchive.Entries;
            }

            public void Dispose()
            {
                _zipArchive.Dispose();
            }

            public XItemMeta GetXItemMeta()
            {
                var entry = _zipArchive.GetEntry(MetaFileName);
                if (entry == null)
                {
                    throw new Exception($"{MetaFileName} not found");
                }

                var xItemBytes = ReadEntryToEnd(entry);
                var json = Encoding.UTF8.GetString(xItemBytes);

                var result = Newtonsoft.Json.JsonConvert.DeserializeObject<XItemMeta>(
                    json,
                    XWearIOUtil.SerializerSetting
                );

                return result;
            }

            public void GetAndSetCurrentMeshInfo(
                XResourceSkinnedMeshRenderer xResourceSkinnedMeshRenderer,
                SkinnedMeshRenderer destSkinnedMeshRenderer,
                bool loadDefault
            )
            {
                var fileName = GetMeshPath(xResourceSkinnedMeshRenderer, isDefault: loadDefault);
                var entry = _zipArchive.GetEntry(fileName);
                if (entry == null)
                {
                    throw new Exception($"{fileName} not found");
                }

                var meshBytes = ReadEntryToEnd(entry);
                using (var meshReader = new MeshInfoReader(meshBytes))
                {
                    meshReader.ReadTo(destSkinnedMeshRenderer);
                }
            }

            public void GetMeshInfo(
                XResourceSkinnedMeshRenderer xResourceSkinnedMeshRenderer,
                UnityEngine.Mesh destMesh,
                bool loadDefault
            )
            {
                var fileName = GetMeshPath(xResourceSkinnedMeshRenderer, isDefault: loadDefault);
                var entry = _zipArchive.GetEntry(fileName);
                if (entry == null)
                {
                    throw new Exception($"{fileName} not found");
                }

                var meshBytes = ReadEntryToEnd(entry);

                using (var meshReader = new MeshInfoReader(meshBytes))
                {
                    meshReader.ReadTo(destMesh, out _);
                }
            }

            public List<bool> GetVertexActiveMarks(
                XResourceSkinnedMeshRenderer xResourceSkinnedMeshRenderer,
                int vertexCount
            )
            {
                var result = new List<bool>();
                var fileName = GetVertexActiveMarksPath(xResourceSkinnedMeshRenderer);
                var entry = _zipArchive.GetEntry(fileName);
                if (entry == null)
                {
                    // entryが見つからなければ全てアクティブな状態であるとして返却する
                    for (int i = 0; i < vertexCount; i++)
                    {
                        result.Add(true);
                    }

                    return result;
                }

                var binary = ReadEntryToEnd(entry);

                using (var ms = new MemoryStream(binary))
                {
                    using (var br = new BinaryReader(ms))
                    {
                        for (int i = 0; i < vertexCount; i++)
                        {
                            result.Add(br.ReadBoolean());
                        }
                    }
                }

                return result;
            }

            public XItem GetXItem(XItemMeta.XItemInfo xItemInfo)
            {
                var fileName = GenerateXItemFileName(xItemInfo.guid);
                var entry = _zipArchive.GetEntry(fileName);
                if (entry == null)
                {
                    UnityEngine.Debug.LogWarning($"{fileName} not found");
                    return null;
                }

                var xItemBytes = ReadEntryToEnd(entry);
                var json = Encoding.UTF8.GetString(xItemBytes);

                XItem xItem;

                if (
                    xItemInfo.xItemType == XItemMeta.XItemType.Avatar
                    || xItemInfo.xItemType == XItemMeta.XItemType.BaseModel
                )
                {
                    xItem = Newtonsoft.Json.JsonConvert.DeserializeObject<AvatarXItem>(
                        json,
                        XWearIOUtil.SerializerSetting
                    );
                }
                else
                {
                    xItem = Newtonsoft.Json.JsonConvert.DeserializeObject<WearXItem>(
                        json,
                        XWearIOUtil.SerializerSetting
                    );
                }

                return xItem;
            }

            public UnityEngine.Texture GetXResourceTextureAndCreate(
                XResourceTexture xResourceTexture
            )
            {
                var textureBytes = GetXResourceTextureBinary(xResourceTexture);
                var resultTexture = TextureUtil.CreateTexture(xResourceTexture, textureBytes);

                return resultTexture;
            }

            public byte[] GetXResourceTextureBinary(XResourceTexture xResourceTexture)
            {
                var fileName = GetTexturePath(xResourceTexture);
                var entry = _zipArchive.GetEntry(fileName);
                var textureBytes = ReadEntryToEnd(entry);
                return textureBytes;
            }

            public UnityEngine.Texture2D GetThumbnailAndCreate()
            {
                var entry = _zipArchive.GetEntry(ThumbnailFileName);
                var thumbnailBinary = ReadEntryToEnd(entry);

                return ThumbnailUtil.CreateThumbnailTexture(thumbnailBinary);
            }

            public byte[] ExtractVrcAssetResources(VrcAssetResource vrcAssetResource)
            {
                var assetType = vrcAssetResource.type;
                var guid = vrcAssetResource.Guid;
                var entryName = Path.Combine(GetAssetResourceFolder(assetType), guid);
                var entry = _zipArchive.GetEntry(entryName);
                return ReadEntryToEnd(entry);
            }

            public List<XResourceAnimationClip> GetAllXResourceAnimationClips()
            {
                var result = new List<XResourceAnimationClip>();
                var animationClipFolder = GetAnimationClipFolder();
                foreach (var entry in _entries)
                {
                    if (entry.FullName.StartsWith(animationClipFolder))
                    {
                        var clipBinary = ReadEntryToEnd(entry);
                        var json = Encoding.UTF8.GetString(clipBinary);

                        var clipResource =
                            Newtonsoft.Json.JsonConvert.DeserializeObject<XResourceAnimationClip>(
                                json,
                                XWearIOUtil.SerializerSetting
                            );

                        result.Add(clipResource);
                    }
                }

                return result;
            }

            public void ReadAllExternalResources(Action<string, byte[]> onRead)
            {
                var externalEntries = _entries.Where(
                    x => x.FullName.StartsWith(ExternalResourceFolderName)
                );
                foreach (var externalEntry in externalEntries)
                {
                    var externalBinary = ReadEntryToEnd(externalEntry);
                    onRead?.Invoke(externalEntry.FullName, externalBinary);
                }
            }

            private byte[] ReadEntryToEnd(ZipArchiveEntry entry)
            {
                var buffer = new byte[entry.Length];
                using (var es = entry.Open())
                {
                    var offset = 0;
                    while (offset < buffer.Length)
                    {
                        var count = buffer.Length - offset;
                        var bytesRead = es.Read(buffer, offset, count);
                        if (bytesRead <= 0)
                        {
                            break;
                        }

                        offset += bytesRead;
                    }
                }

                return buffer;
            }
        }

        public class XResourceArchiver : IDisposable
        {
            private readonly ZipArchive _zipArchive;
            public readonly bool ArchiveFromPackage;

            public XResourceArchiver(FileStream fileStream, bool archiveFromPackage)
            {
                _zipArchive = new ZipArchive(
                    fileStream,
                    ZipArchiveMode.Create,
                    true,
                    Encoding.UTF8
                );
                ArchiveFromPackage = archiveFromPackage;
            }

            public void AddMeta(XItemMeta meta)
            {
                // このタイミングでバージョンを書くことでランタイム・Editor両方でのバージョンの書き洩らしを防ぐ
                meta.version = XItemMeta.CurrentVersion;

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(
                    meta,
                    XWearIOUtil.SerializerSetting
                );

                var metaJsonBytes = Encoding.UTF8.GetBytes(json);

                AddEntry(MetaFileName, metaJsonBytes);
            }

            public void AddXItem(XItem xItem)
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(
                    xItem,
                    XWearIOUtil.SerializerSetting
                );

                var metaJsonBytes = Encoding.UTF8.GetBytes(json);
                AddEntry(GenerateXItemFileName(xItem.Guid), metaJsonBytes);
            }

            public void AddXResourceTexture(XResourceTexture xResourceTexture, byte[] textureBytes)
            {
                var fileName = GetTexturePath(xResourceTexture);
                AddEntry(fileName, textureBytes);
            }

            public void AddThumbnail(UnityEngine.GameObject takeThumbnailTarget)
            {
                var thumbnailBinary = ThumbnailTaker.TakeThumbnail(takeThumbnailTarget);
                AddEntry(ThumbnailFileName, thumbnailBinary);
            }

            public void AddMesh(
                XResourceSkinnedMeshRenderer xResourceSkinnedMeshRenderer,
                SkinnedMeshRenderer skinnedMeshRenderer,
                bool isDefault
            )
            {
                byte[] meshBinary;
                using (var meshInfoWriter = new MeshInfoWriter())
                {
                    // isDefaultがtrueのとき、ブレンドシェイプは全て0の状態とする
                    meshInfoWriter.Write(skinnedMeshRenderer, setZeroBlendShape: isDefault);
                    meshBinary = meshInfoWriter.GetCurrent();
                }

                var fileName = GetMeshPath(xResourceSkinnedMeshRenderer, isDefault: isDefault);

                AddEntry(fileName, meshBinary);
            }

            public void AddMesh(
                XResourceSkinnedMeshRenderer xResourceSkinnedMeshRenderer,
                UnityEngine.Mesh mesh,
                bool isDefault
            )
            {
                byte[] meshBinary;
                using (var meshInfoWriter = new MeshInfoWriter())
                {
                    meshInfoWriter.Write(mesh);
                    meshBinary = meshInfoWriter.GetCurrent();
                }

                var fileName = GetMeshPath(xResourceSkinnedMeshRenderer, isDefault: isDefault);

                AddEntry(fileName, meshBinary);
            }

            public void AddVertexActiveMarks(
                XResourceSkinnedMeshRenderer xResourceSkinnedMeshRenderer,
                List<bool> deleteVertexInfo
            )
            {
                byte[] binary;
                using (var ms = new MemoryStream())
                {
                    using (var bw = new BinaryWriter(ms))
                    {
                        foreach (var info in deleteVertexInfo)
                        {
                            bw.Write(info);
                        }

                        binary = ms.ToArray();
                    }
                }

                var fileName = GetVertexActiveMarksPath(xResourceSkinnedMeshRenderer);
                AddEntry(fileName, binary);
            }

            public void AddVrcAssetResources(VrcAssetResource assetResource, string filePath)
            {
                var guid = assetResource.Guid;
                var assetType = assetResource.type;

                var binary = File.ReadAllBytes(filePath);
                var entryName = Path.Combine(GetAssetResourceFolder(assetType), guid);
                AddEntry(entryName, binary);
            }

            public void AddXResourceAnimationClips(XResourceAnimationClip[] clips)
            {
                foreach (var clip in clips)
                {
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(
                        clip,
                        XWearIOUtil.SerializerSetting
                    );

                    var jsonBytes = Encoding.UTF8.GetBytes(json);
                    AddEntry(Path.Combine(GetAnimationClipFolder(), clip.guid), jsonBytes);
                }
            }

            public void AddFile(string filePath)
            {
                var binary = File.ReadAllBytes(filePath);
                AddEntry(Path.GetFileName(filePath), binary);
            }

            public void AddEntry(string entryName, byte[] bytes)
            {
                var entry = _zipArchive.CreateEntry(entryName);
                using (var es = entry.Open())
                {
                    es.Write(bytes, 0, bytes.Length);
                }
            }

            public void Dispose()
            {
                _zipArchive.Dispose();
            }
        }
    }
}
