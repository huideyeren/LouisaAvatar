using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// ver 0.0.1
// Copyright (c) 2022 TKSP

// 名前空間の定義
namespace VGC
{
    public static class VGCLocalizeHelper
    {
        public enum Language
        {
            Japanese = 0,
            English,
            Korean,
        };

        // ここが増えると選択肢が増えるので注意
        private static string[] _languageCSVList = {
                "jp.csv",
                "en.csv",
                "kr.csv",
        };

        // メニュー設定
        private static string[] _languageArray = { "日本語", "English", "한국어" };

        // 現在選択中の言語(念のためpublicにしているが、できれば使用しない)
        public static Language language = Language.Japanese;

        /// <summary>
        /// ローカライズテキストの取得
        /// </summary>
        /// <param name="csvFolderPath">csv格納フォルダのパス</param>
        /// <returns>読み込んだ文字列の配列</returns>
        public static List<string> GetLocalizeTexts(string csvFolderPath)
        {
            List<string> retTexts = new List<string>();

            string csvPath = csvFolderPath + "/" + _languageCSVList[(int)language];

            var csvFile = AssetDatabase.LoadAssetAtPath<TextAsset>(csvPath);

            if (!csvFile)
            {
                csvPath = csvFolderPath + "/" + _languageCSVList[(int)Language.English];
                csvFile = AssetDatabase.LoadAssetAtPath<TextAsset>(csvPath);
            }

            if (csvFile)
            {
                StringReader reader = new StringReader(csvFile.text);

                while (reader.Peek() > -1)
                {
                    // 一行読み込み
                    var text = reader.ReadLine();
                    if (text.Length == 0)
                        break;
                    text = text.Replace(@"\n", Environment.NewLine);
                    retTexts.Add(text);
                }
            }
            return retTexts;
        }

        /// <summary>
        /// テキストを取得する
        /// </summary>
        /// <param name="jp">日本語</param>
        /// <param name="en">英語</param>
        /// <param name="kr">中国語</param>
        /// <returns></returns>
        public static string convertLanguage(string jp, string en, string kr)
        {
            switch (language)
            {
                case Language.Japanese:
                    return jp;
                case Language.English:
                    return en;
                case Language.Korean:
                    return kr;
                default:
                    return en;
            }
        }

        /// <summary>
        /// 言語選択ウィンドウ表示
        /// </summary>
        /// <param name="csvFolderPath">csv格納フォルダのパス</param>
        /// <returns>対応している言語が選択されているか</returns>
        public static bool ShowEditLanguage(string csvFolderPath)
        {
            // 対応している言語のリスト
            List<string> handleLanguages = new List<string>();
            List<bool> isSupportList = new List<bool>();
            for (int i = 0; i < _languageCSVList.Length; i++)
            {
                string csvPath = csvFolderPath + "/" + _languageCSVList[i];
                var csvFile = AssetDatabase.LoadAssetAtPath<TextAsset>(csvPath);

                // 存在確認
                isSupportList.Add(csvFile);
                if (csvFile)
                {
                    handleLanguages.Add(_languageArray[i]);
                }
                else
                {
                    handleLanguages.Add("Languages not supported by this tool");
                }
            }

            language = (Language)Enum.ToObject(typeof(Language), EditorGUILayout.Popup("Language", (int)language, handleLanguages.ToArray()));

            // サポートしていない言語が選択されている
            if (!isSupportList[(int)language])
            {
                GUILayout.Label("※Unsupported language! Please select other language.");
                return false;
            }

            return true;
        }
    }
}