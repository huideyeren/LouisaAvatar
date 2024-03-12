using System;
using UnityEngine.Serialization;

namespace XWear.IO.XResource.Licensing
{
    [Serializable]
    public class XResourceLicense
    {
        public enum LicenseType
        {
            Text,
            File,
            Url
        }

        public LicenseType licenseType;
        public string author;
        public string licenseText;

        public byte[] licenseFileBinary;
        public string licenseFileName;
    }
}
