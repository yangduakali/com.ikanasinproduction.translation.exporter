using ikanAsin.localization;
using Newtonsoft.Json;
using TranslationExporter;
using UnityEngine;

namespace cisini.TranslationExporter
{
    public class LocalizeString : ILocalizeString
    {
        public string Key { get; set; }
        [JsonIgnore] public Object Asset;
        [JsonProperty("Indonesian(id)")] public string Indonesian;
        [JsonProperty("English(en)")] public string English;

        public Object GetAsset()
        {
            return Asset;
        }

        public void SetValue(Localize localize, string value)
        {
            if (localize == Localize.Indonesia)
            {
                Indonesian = value;
            }
            else
            {
                English = value;
            }
        }
    }
}