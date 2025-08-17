using System.Collections.Generic;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;

namespace citampi.TranslationExporter
{
    public class ExtraDialogueProcessor : ResourcesJsonExporterScriptable<extraDialogue>
    {
        public override string GroupId => "ExtraDialogue";
        protected override string ResourcesPath => "ExtraDialogues";

        protected override LocalizeString[] GetLocalString(extraDialogue asset, out List<object> metadata)
        {
            var result = new List<LocalizeString>();
            metadata = new List<object>();
            var npcDialogue = asset.NPCDialogue;

            for (int i = 0; i < npcDialogue.Length; i++)
            {
                var dialogue = npcDialogue[i];
                if (dialogue.dialogueScript.Length == 0) continue;

                for (int j = 0; j < dialogue.dialogueScript.Length; j++)
                {
                    var script = dialogue.dialogueScript[j];
                    metadata.Add(new LocalizeData
                    {
                        IndexNPCDialogue = i,
                        IndexDialogueScript = j,
                    });
                    result.Add(GetLocalizeStringFromStringArray(script.TextByLanguage, asset));
                }
            }

            return result.ToArray();
        }

        protected override void UpdateData(extraDialogue asset, object columnKey, object value, object metadata)
        {
            var data = ConvertTo<LocalizeData>(metadata);

            if (data.IndexNPCDialogue >= asset.NPCDialogue.Length)
            {
                Debug.Log(
                    $"Cannot import asset {asset.name}. NPCDialogue : index out of range ({data.IndexNPCDialogue}))");
                return;
            }
            
            var npcDialogue = asset.NPCDialogue[data.IndexNPCDialogue];
            if (data.IndexDialogueScript >= npcDialogue.dialogueScript.Length)
            {
                Debug.Log(
                    $"Cannot import asset {asset.name}. NPCDialogue[${data.IndexNPCDialogue}].dialogueScript : index out of range");
                return;
            }
            
            var script = npcDialogue.dialogueScript[data.IndexDialogueScript];
            script.TextByLanguage[GetLocalizeIndexFromColumnKey(columnKey)] = $"{value}";
        }
        
        private struct LocalizeData
        {
            public int IndexNPCDialogue;
            public int IndexDialogueScript;
        }
    }
    
    
}

