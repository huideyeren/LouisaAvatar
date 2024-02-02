using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using XWear.IO.XResource.Licensing;

namespace XWear.IO
{
    [Serializable]
    public class ExportContext
    {
        public enum ExportType
        {
            Avatar,
            Wear,
            Accessory
        }

        public ExportType exportType;
        public GameObject exportRoot;
        public List<GameObject> exportChildren = new List<GameObject>();
        public XResourceLicenseObject licenseObject;
    }

    [Serializable]
    public class ImportContext
    {
        public enum ImportType
        {
            Avatar,
            Wear,
            Accessory
        }

        public ImportContext importType;
    }
}
