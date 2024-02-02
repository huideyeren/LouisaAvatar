using UnityEngine;

namespace XWear.IO.XResource.Licensing
{
    [CreateAssetMenu(menuName = "VRoid/CreateLicenseObject")]
    public class XResourceLicenseObject : ScriptableObject
    {
        public XResourceLicense license;
    }
}
