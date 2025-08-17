using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TranslationExporter;
using UnityEditor;
using UnityEngine;

namespace citampi.TranslationExporter
{
    public class TextLibraryProcessor : ResourcesJsonExporter
    {
        public override string GroupId => "TextLib";
        private readonly Dictionary<string, LocalizeText> _localizeTexts = new(); 
        private string _resourcePath = $"{Application.dataPath}/Resources/TextLibaryJson.json";
        protected override List<ILocalizeString> GetLocalizeData()
        {
            _localizeTexts.Clear();
            var result = new List<ILocalizeString>();
            var asset = Resources.Load<TextAsset>("TextLibaryJson");
            if (asset == null) return result;
            var localizeTexts = JsonConvert.DeserializeObject<LocalizeText[]>(asset.text);
            var newLocalizeTexts = new List<LocalizeText>();
            foreach (var localizeText in localizeTexts)
            {
                if (_localizeTexts.ContainsKey(localizeText.TextName))
                {
                    Debug.Log($"Duplicate id TextLibrary {localizeText.TextName}. Skipping...");
                    continue;
                };
                _localizeTexts.Add(localizeText.TextName, localizeText);
                newLocalizeTexts.Add(localizeText);
                var localizeString = GetLocalizeStringFromStringArray(localizeText.TextByLanguage);
                localizeString.Key = localizeText.TextName;
                result.Add(localizeString);
            }
            File.WriteAllText(_resourcePath, JsonConvert.SerializeObject(newLocalizeTexts.ToArray(), Formatting.Indented));
            return result;
        }

        protected override void UpdateValue(object rowKey, object columnKey, object value)
        {
            if(!_localizeTexts.TryGetValue($"{rowKey}", out var localizeText)) return;
            var index = GetLocalizeIndexFromColumnKey(columnKey);
            
            var stringArray = localizeText.TextByLanguage;
            stringArray[index] = $"{value}";
            localizeText.TextByLanguage = stringArray;
            _localizeTexts[$"{rowKey}"] = localizeText;
            
            var jsonString = JsonConvert.SerializeObject(_localizeTexts.Values.ToArray(), Formatting.Indented);
            File.WriteAllText(_resourcePath, jsonString);
        }

        private struct LocalizeText
        {
            public string TextName;
            public string[] TextByLanguage; 
        }
    }
}