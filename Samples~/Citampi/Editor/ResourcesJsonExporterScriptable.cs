using System.Collections.Generic;
using TranslationExporter;
using UnityEditor;
using UnityEngine;

namespace citampi.TranslationExporter
{
    /// <summary>
    /// Base exporter that serializes localization data from <see cref="ScriptableObject"/> assets
    /// into JSON, and manages synchronization between assets and spreadsheet metadata.  
    /// Provides hooks for extracting localized strings and updating asset values.
    /// </summary>
    /// <typeparam name="T">The type of ScriptableObject being exported.</typeparam>
    public abstract class ResourcesJsonExporterScriptable<T> : ResourcesJsonExporter where T : ScriptableObject
    {
        protected abstract string ResourcesPath { get; }

        protected sealed override List<ILocalizeString> GetLocalizeData()
        {
            var lookUpId = GetAssetLookUpId();
            var assetById = lookUpId.SwapKey();
            var dataByAsset = new Dictionary<T, DataFile>();
            foreach (var (assetId, data) in Metadata.Entries)
            {
                lookUpId.entries.TryGetValue(assetId, out var asset);
                {
                    if(asset is null) continue;
                    dataByAsset.Add((T)asset,data);                    
                }
            }

            Metadata.Entries.Clear();
            lookUpId.entries.Clear();
            var result = new List<ILocalizeString>();
            var assets = Resources.LoadAll<T>(ResourcesPath);
            foreach (var asset in assets)
            {
                // get or create asset id
                if (!assetById.Remove(asset, out var assetId))
                {
                    assetId = Metadata.GetNewId();
                }
                lookUpId.entries.Add(assetId, asset);
                
                var rowExist = dataByAsset.Remove(asset, out var data);
                var localStrings = GetLocalString(asset, out var metadata);
                var localizeData = new List<LocalizeData>();
                
                for (int i = 0; i < localStrings.Length; i++)
                {
                    string key;
                    if (rowExist && i < data.localizeData.Length)
                    {
                        key = data.localizeData[i].key;
                    }
                    else
                    {
                        key = $"{GroupId}-{assetId}-{Metadata.GetNewId()}";
                    }
                
                    localizeData.Add(new()
                    {
                        key = key,
                        data = metadata[i]
                    });
                    var localizeString = localStrings[i];
                    localizeString.Key = key;
                    result.Add(localizeString);
                }
                
                var newData = new DataFile()
                {
                    AssetId = assetId,
                    localizeData = localizeData.ToArray(),
                };
                Metadata.Entries.Add(assetId, newData);
            }
            SaveJsonMetadata();
            EditorUtility.SetDirty(lookUpId);
            return result;
        }

        protected sealed override void UpdateValue(object rowKey, object columnKey, object value)
        {
            if (!$"{rowKey}".StartsWith(GroupId)) return;
            var splitKey = $"{rowKey}".Split('-');
            if (splitKey.Length < 2) return;

            if(!int.TryParse(splitKey[1], out var assetId)) return;
            if (!Metadata.Entries.ContainsKey(assetId)) return;
            var lookUpId = GetAssetLookUpId();
            var data = Metadata.Entries[assetId];
            if(!lookUpId.entries.TryGetValue(data.AssetId, out var asset)) return;
            if (asset is null) return;

            foreach (var localizeData in data.localizeData)
            {
                if (localizeData.key != $"{rowKey}") continue;
                UpdateData((T)asset, columnKey, value, localizeData.data);
                EditorUtility.SetDirty(asset);
                break;
            }
        }

        /// <summary>
        /// Extracts all localized strings from a given asset.  
        /// Each localized string must have a matching metadata entry (1:1 relationship).  
        /// Metadata is used later to identify and update the correct fields inside the asset.  
        /// </summary>
        /// <param name="asset">The asset to extract strings from.</param>
        /// <param name="metadata">
        /// A parallel list of metadata objects for each returned <see cref="LocalizeString"/>.  
        /// Must have the same length and order as the returned array, even if some fields require no metadata.
        /// </param>
        /// <returns>
        /// An array of <see cref="LocalizeString"/> entries representing the translatable fields of the asset.
        /// </returns>
        protected abstract LocalizeString[] GetLocalString(T asset, out List<object> metadata);

        /// <summary>
        /// Applies an updated value from the spreadsheet back into the asset.  
        /// The correct field should be identified using <paramref name="metadata"/>.  
        /// </summary>
        /// <param name="asset">The target asset instance to modify.</param>
        /// <param name="columnKey">The spreadsheet column key (e.g., language name).</param>
        /// <param name="value">The new value to set.</param>
        /// <param name="metadata">
        /// A metadata object previously provided by <see cref="GetLocalString"/>  
        /// that identifies which field/property in the asset should be updated.
        /// </param>
        protected abstract void UpdateData(T asset, object columnKey, object value, object metadata);

        protected AssetLookUpId GetAssetLookUpId()
        {
            var assetPath = $"Assets/{ResourcesJsonFolderPath}/{GroupId}.asset";
            var result = AssetDatabase.LoadAssetAtPath<AssetLookUpId>(assetPath);
            if(result is not null) return result;
            result = ScriptableObject.CreateInstance<AssetLookUpId>();
            AssetDatabase.CreateAsset(result, assetPath);
            result.entries ??= new();
            return result;
        }
    }
}