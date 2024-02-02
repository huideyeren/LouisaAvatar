using System.Collections.Generic;
using UnityEngine;
using XWear.IO.XResource.Accessory;
using XWear.IO.XResource.Humanoid;
using XWear.IO.XResource.Transform;

namespace XWear.IO.XResource
{
    public abstract class XResource
    {
        public XResourceGameObject Root;
        public List<XResourceGameObject> GameObjects = new List<XResourceGameObject>();
    }

    public class HumanoidXResource : XResource
    {
        public XResourceHumanoidMap HumanoidMap;
    }

    public class AccessoryXResource : XResource
    {
        public XResourceAccessoryMap AccessoryMap;
    }

    public interface IXResourceInstance
    {
        string Guid { get; }
        GameObject Instance { get; }
        XResource XResourece { get; }
        List<UnityEngine.Component> Components { get; }
    }

    public class HumanoidXResourceInstance : HumanoidXResource, IXResourceInstance
    {
        public HumanoidMapComponent HumanoidMapComponent;
        public string Guid { get; }
        public GameObject Instance { get; }
        public XResource XResourece { get; }
        public List<UnityEngine.Component> Components { get; }

        /// <summary>
        /// XResourceTransformに保存されたインデックスからビルドされたTransformの実体を引くマップ
        /// </summary>
        public readonly Dictionary<
            int,
            UnityEngine.Transform
        > XResourceTransformIndexToBuiltTransformEntityMap;

        public HumanoidXResourceInstance(
            XResource xResource,
            GameObject instance,
            List<UnityEngine.Component> components,
            Dictionary<int, UnityEngine.Transform> xResourceTransformIndexToBuiltTransformEntityMap
        )
        {
            Root = xResource.Root;
            GameObjects = xResource.GameObjects;
            Guid = System.Guid.NewGuid().ToString();
            Instance = instance;
            XResourceTransformIndexToBuiltTransformEntityMap =
                xResourceTransformIndexToBuiltTransformEntityMap;
            XResourece = xResource;
            Components = components;
        }
    }

    public class AccessoryXResourceInstance : AccessoryXResource, IXResourceInstance
    {
        public AccessoryMapComponent AccessoryMapComponent;
        public string Guid { get; }
        public GameObject Instance { get; }
        public XResource XResourece { get; }
        public List<UnityEngine.Component> Components { get; }

        public AccessoryXResourceInstance(XResource xResource, GameObject instance)
        {
            Root = xResource.Root;
            GameObjects = xResource.GameObjects;
            Guid = System.Guid.NewGuid().ToString();
            Instance = instance;
            XResourece = xResource;
        }
    }
}
