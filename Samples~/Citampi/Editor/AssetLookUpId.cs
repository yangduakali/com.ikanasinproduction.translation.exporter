using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace citampi.TranslationExporter
{
    public class AssetLookUpId : ScriptableObject
    {
        [ReadOnly] public SerializedDictionary<int, ScriptableObject> entries = new();

        public Dictionary<ScriptableObject, int> SwapKey()
        {
            var result = new Dictionary<ScriptableObject, int>();
            foreach (var (key, value) in entries)
            {
                result.Add(value, key);
            }

            return result;
        }
    }
}