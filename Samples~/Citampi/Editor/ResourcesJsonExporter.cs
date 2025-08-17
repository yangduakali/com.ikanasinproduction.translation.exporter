using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TranslationExporter;
using UnityEditor;
using UnityEngine;

namespace citampi.TranslationExporter
{
    public abstract class ResourcesJsonExporter : ExporterProcessor
    {

        /// <summary>
        /// Relative path to the folder where exported JSON files will be stored,
        /// starting from "Assets/".
        /// </summary>
        protected const string ResourcesJsonFolderPath = "TranslationExporter/Editor/Citampi/ResourcesJson";

        protected ProcessorMetadata Metadata => _metadata ?? GetJsonMetadata();

        private ProcessorMetadata _metadata;

        private ProcessorMetadata GetJsonMetadata()
        {
            var json = AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/{ResourcesJsonFolderPath}/{GroupId}.json");
            
            _metadata = json == null
                ? new ProcessorMetadata()
                : JsonConvert.DeserializeObject<ProcessorMetadata>(json.text);
            SaveJsonMetadata();
            return _metadata;
        }

        protected void SaveJsonMetadata()
        {
            var path = $"{Application.dataPath}/{ResourcesJsonFolderPath}/{GroupId}.json";
            CreateFolder($"Assets/{ResourcesJsonFolderPath}");
            File.WriteAllText(path, JsonConvert.SerializeObject(_metadata ?? new ProcessorMetadata()));
        }

        /// <summary>
        /// Creates a folder (and all parent folders if necessary) at the specified project-relative path.
        /// </summary>
        /// <param name="path">A Unity project-relative path (e.g., "Assets/Folder1/Folder2").</param>
        protected static void CreateFolder(string path)
        {
            var p = path.Split("/");
            var r = "";
            for (int i = 0; i < p.Length; i++)
            {
                if (AssetDatabase.IsValidFolder(r + "/" + p[i]))
                {
                    if (i > 0)
                    {
                        r += "/";
                    }

                    r += p[i];
                    continue;
                }

                // Create the folder in Unity's AssetDatabase
                AssetDatabase.CreateFolder(r, p[i]);
                if (i > 0)
                {
                    r += "/";
                }

                r += p[i];
            }
        }

        /// <summary>
        /// Creates a <see cref="LocalizeString"/> from an array of 8 localized strings.
        /// Order: [0] English, [1] Indonesian, [2] Filipino, [3] Thai, [4] Vietnamese, [5] Portuguese, [6] Spanish, [7] French.
        /// </summary>
        /// <param name="array">An array of localized strings (must be exactly 8 elements).</param>
        /// <param name="asset">Optional, to display asset on Diff View</param>
        /// <returns>A <see cref="LocalizeString"/> containing the values from the array, or an empty one if invalid.</returns>
        protected LocalizeString GetLocalizeStringFromStringArray(string[] array, Object asset = null)
        {
            if (array is not { Length: 8 })
            {
                return new LocalizeString
                {
                    Asset = asset
                };
            }

            return new LocalizeString
            {
                Asset = asset,
                English = array[0],
                Indonesian = array[1],
                Filipino = array[2],
                Thai = array[3],
                Vietnamese = array[4],
                Portuguese = array[5],
                Spanish = array[6],
                French = array[7],
            };
        }

        protected int GetLocalizeIndexFromColumnKey(object columnKey)
        {
            return $"{columnKey}" switch
            {
                "English(en)" => 0,
                "Indonesian(id)" => 1,
                "Filipino(tl)" => 2,
                "Thai(th)" => 3,
                "Vietnamese(vi)" => 4,
                "Portuguese(pt)" => 5,
                "Spanish(es)" => 6,
                "French(fr)" => 7,
                _ => 0
            };
        }

        protected T ConvertTo<T>(object value)
        {
            return value switch
            {
                T result => result,
                null => default,
                _ => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(value))
            };
        }
    }

    public struct DataFile
    {
        public int AssetId;
        public LocalizeData[] localizeData;
    }

    public struct LocalizeData
    {
        public string key;
        public object data;
    }

    public class ProcessorMetadata
    {
        public int IdCounter;
        public readonly Dictionary<int, DataFile> Entries = new();

        public int GetNewId()
        {
            IdCounter++;
            return IdCounter;
        }
    }

    public class LocalizeString : ILocalizeString
    {
        public string Key { get; set; }
        [JsonIgnore] public Object Asset;
        [JsonProperty("English(en)")] public string English;
        [JsonProperty("Indonesian(id)")] public string Indonesian;
        [JsonProperty("Filipino(tl)")] public string Filipino;
        [JsonProperty("Thai(th)")] public string Thai;
        [JsonProperty("Vietnamese(vi)")] public string Vietnamese;
        [JsonProperty("Portuguese(pt)")] public string Portuguese;
        [JsonProperty("Spanish(es)")] public string Spanish;
        [JsonProperty("French(fr)")] public string French;

        public Object GetAsset()
        {
            return Asset;
        }

        public LocalizeString WithAsset(Object asset)
        {
            Asset = asset;
            return this;
        }
    }
}