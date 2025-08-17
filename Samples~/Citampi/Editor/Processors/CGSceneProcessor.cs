using System.Collections.Generic;
using UnityEngine;

namespace citampi.TranslationExporter
{
    public class CGSceneProcessor : ResourcesJsonExporterScriptable<CGScene>
    {
        public override string GroupId => "CGScene";
        protected override string ResourcesPath => "CGScene";

        protected override LocalizeString[] GetLocalString(CGScene asset, out List<object> metadata)
        {
            var result = new List<LocalizeString>();
            metadata = new List<object>();

            for (var i = 0; i < asset.cutsceneDialogues.Count; i++)
            {
                var dialogue = asset.cutsceneDialogues[i];
                if (dialogue.TextByLanguage.Length == 0) continue;
                metadata.Add(new LocalizeData
                {
                    Index = i
                });
                result.Add(GetLocalizeStringFromStringArray(dialogue.TextByLanguage, asset));
            }

            return result.ToArray();
        }

        protected override void UpdateData(CGScene asset, object columnKey, object value, object metadata)
        {
            var data = ConvertTo<LocalizeData>(metadata);
            if (data.Index >= asset.cutsceneDialogues.Count)
            {
                Debug.Log(
                    $"Cannot update {asset.name}. {nameof(asset.cutsceneDialogues)} : index out of range ");
                return;
            }
            asset.cutsceneDialogues[data.Index].TextByLanguage[GetLocalizeIndexFromColumnKey(columnKey)] = $"{value}";
        }

        private struct LocalizeData
        {
            public int Index;
        }
    }
}