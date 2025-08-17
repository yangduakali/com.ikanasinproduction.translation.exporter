using System.Collections.Generic;
using UnityEngine;

namespace citampi.TranslationExporter
{
    public class CharacterConversationProcessor : ResourcesJsonExporterScriptable<CharacterConversation>
    {
        public override string GroupId => "CharConv";
        protected override string ResourcesPath => "Conversation";

        protected override LocalizeString[] GetLocalString(CharacterConversation asset, out List<object> metadata)
        {
            var result = new List<LocalizeString>();
            metadata = new List<object>();
            var dialogues = asset.characterDialogues;

            for (int i = 0; i < dialogues.Length; i++)
            {
                var dialogue = dialogues[i];
                if (dialogue.dialogueScript.Length == 0) continue;

                for (int j = 0; j < dialogue.dialogueScript.Length; j++)
                {
                    var script = dialogue.dialogueScript[j];
                    metadata.Add(new LocalizeData
                    {
                        IndexDialogue = i,
                        IndexScript = j,
                    });
                    result.Add(GetLocalizeStringFromStringArray(script.TextByLanguage, asset));
                }
            }

            return result.ToArray();
        }

        protected override void UpdateData(CharacterConversation asset, object columnKey, object value, object metadata)
        {
            var data = ConvertTo<LocalizeData>(metadata);

            if (data.IndexDialogue >= asset.characterDialogues.Length)
            {
                Debug.Log(
                    $"Cannot update {asset.name}. {nameof(asset.characterDialogues)} : index out of range");
                return;
            }

            var npcDialogue = asset.characterDialogues[data.IndexDialogue];
            if (data.IndexScript >= npcDialogue.dialogueScript.Length)
            {
                Debug.Log(
                    $"Cannot update {asset.name}. {nameof(npcDialogue.dialogueScript)} : index out of range");
                return;
            }

            var script = npcDialogue.dialogueScript[data.IndexScript];
            script.TextByLanguage[GetLocalizeIndexFromColumnKey(columnKey)] = $"{value}";
        }

        private struct LocalizeData
        {
            public int IndexDialogue;
            public int IndexScript;
        }
    }
}