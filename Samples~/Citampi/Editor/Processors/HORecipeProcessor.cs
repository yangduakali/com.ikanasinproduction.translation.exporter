using System.Collections.Generic;
using MiniGame.HiddenObject;
using UnityEngine;

namespace citampi.TranslationExporter
{
    public class HORecipeProcessor : ResourcesJsonExporterScriptable<HO_Recipe>
    {
        public override string GroupId => "Recipe";
        protected override string ResourcesPath => "Minigame";
        
        protected override LocalizeString[] GetLocalString(HO_Recipe asset, out List<object> metadata)
        {
            var result = new List<LocalizeString>();
            metadata = new List<object>();
            for (var i = 0; i < asset.Dialogues.Count; i++)
            {
                var dialogue = asset.Dialogues[i];
                result.Add(GetLocalizeStringFromStringArray(dialogue.TextByLanguage, asset));
                metadata.Add(new LocalizeData
                {
                    dialogueIndex = i
                });
            }
            return result.ToArray();
        }
        protected override void UpdateData(HO_Recipe asset, object columnKey, object value, object metadata)
        {
            var data = ConvertTo<LocalizeData>(metadata);
            if(data.dialogueIndex >= asset.Dialogues.Count) return;
            var dialogue = asset.Dialogues[data.dialogueIndex];
            dialogue.TextByLanguage[GetLocalizeIndexFromColumnKey(columnKey)] = $"{value}";
        }
        
        private struct LocalizeData
        {
            public int dialogueIndex;
        }
    }
}