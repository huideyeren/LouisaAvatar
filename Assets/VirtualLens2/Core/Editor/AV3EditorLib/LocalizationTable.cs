using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VirtualLens2.AV3EditorLib
{

    /// <summary>
    /// Contains key-value pairs from key string to localized string.
    /// </summary>
    public class LocalizationTable
    {
        private readonly Dictionary<string, string> _table = new Dictionary<string, string>();
        
        /// <summary>
        /// Construct key-value pairs from localization table.
        /// </summary>
        /// <remarks>
        /// Localization tables are tab-separated text file like following:
        /// 
        /// language  en        ja
        /// key1      text1-en  text1-ja
        /// key2      text2-en  text2-ja
        /// </remarks>
        /// <param name="textAsset">The tab-separated text asset.</param>
        /// <param name="language">The language key to build the table.</param>
        public LocalizationTable(TextAsset textAsset, string language)
        {
            using (var sr = new StringReader(textAsset.text))
            {
                var line = sr.ReadLine();
                if (line == null) { return; }
                var languages = line.Split('\t');
                var index = Array.IndexOf(languages, language);
                while ((line = sr.ReadLine()) != null)
                {
                    var elements = line.Split('\t');
                    _table.Add(elements[0], elements[index]);
                }
            }
        }

        /// <summary>
        /// Get localized string from a key string.
        /// </summary>
        /// <param name="key">The key string to get localized string.</param>
        public string this[string key] => _table.ContainsKey(key) ? _table[key] : null;
    }

}