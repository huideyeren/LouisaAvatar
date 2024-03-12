using System.Collections.Generic;
using XWear.IO.XResource.Component;

namespace XWear.IO.XResource.Transform
{
    public class XResourceGameObject
    {
        public string Guid;
        public string Name;
        public bool IsHumanoidBone;
        public string Tag;
        public UnityEngine.HumanBodyBones HumanBodyBones;

        public XResourceTransform Transform = new XResourceTransform();

        public List<XResourceGameObject> Children = new List<XResourceGameObject>();

        public List<IXResourceComponent> Components = new List<IXResourceComponent>();
        public bool ActiveSelf = true;

        public XResourceGameObject() { }
    }
}
