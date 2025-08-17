using System.Collections.Generic;

namespace citampi.TranslationExporter
{
    public class ExtraNPCProcessor : ResourcesJsonExporterScriptable<extraNPC>
    {
        public override string GroupId => "ExtraNPC";
        protected override string ResourcesPath => "extraNPC";

        protected override LocalizeString[] GetLocalString(extraNPC asset, out List<object> metadata)
        {
            var result = new List<LocalizeString>();
            metadata = new List<object>();
            var tempMetadata = new List<object>();


            if (asset.NPCFunction == extraNPCFunction.normal)
            {
                AddFromExtraDialogueArray(asset.NPCDialogue, "NPCDialogue");
            }

            if (asset.NPCFunction == extraNPCFunction.unlistedQuestGiver)
            {
                AddFromExtraDialogueArray(asset.requestDialogue, "requestDialogue");
                AddFromExtraDialogueArray(asset.requestUnDoneDialogue, "requestUnDoneDialogue");
                AddFromDialogueArray(asset.requestDoneDialogue, "requestDoneDialogue");
                AddFromDialogueArray(asset.questRequirementRejectedDialogue, "questRequirementRejectedDialogue");
                AddFromExtraDialogueArray(asset.AfterRequestDialogue, "AfterRequestDialogue");
            }

            if (asset.NPCFunction == extraNPCFunction.conditionQuestGiver)
            {
                AddFromExtraDialogueArray(asset.NPCDialogueCondition, "NPCDialogueCondition");
            }

            if (asset.NPCFunction == extraNPCFunction.EventGateway)
            {
                AddFromExtraDialogueArray(asset.NPCDialogueEvent, "NPCDialogueEvent");
            }

            if (asset.NPCFunction == extraNPCFunction.GiveItem)
            {
                AddFromExtraDialogueArray(asset.giveItemNPCDialogue, "giveItemNPCDialogue");
                AddFromExtraDialogueArray(asset.itemGivenNPCDialogue, "itemGivenNPCDialogue");
            }

            if (asset.NPCFunction == extraNPCFunction.WifeChat)
            {
                AddFromExtraDialogueArray(asset.wifeNabila, "wifeNabila");
                AddFromExtraDialogueArray(asset.wifeIsma, "wifeIsma");
                AddFromExtraDialogueArray(asset.wifeSarah, "wifeSarah");
                AddFromExtraDialogueArray(asset.wifeWindy, "wifeWindy");
                AddFromExtraDialogueArray(asset.wifeCitra, "wifeCitra");
                AddFromExtraDialogueArray(asset.wifeJessica, "wifeJessica");
                AddFromExtraDialogueArray(asset.wifeTasya, "wifeTasya");
                AddFromExtraDialogueArray(asset.wifeIndah, "wifeIndah");
                AddFromExtraDialogueArray(asset.wifeNissa, "wifeNissa");
                AddFromExtraDialogueArray(asset.wifeAsih, "wifeAsih");
                AddFromExtraDialogueArray(asset.wifeMilah, "wifeMilah");
                AddFromExtraDialogueArray(asset.wifeImas, "wifeImas");
            }

            metadata.AddRange(tempMetadata);
            return result.ToArray();

            void AddFromExtraDialogueArray(extraNPCDialogue[] dialogue, string type)
            {
                var localizeStrings = GetLocalStringFromExtraNPCDialogArray(dialogue,
                    type, out var metas);
                result.AddRange(localizeStrings);
                tempMetadata.AddRange(metas);
            }


            void AddFromDialogueArray(Dialogue[] dialogue, string type)
            {
                var localizeStrings = GetLocalStringFromDialogArray(dialogue,
                    type, out var metas);
                result.AddRange(localizeStrings);
                tempMetadata.AddRange(metas);
            }
        }

        private List<LocalizeString> GetLocalStringFromExtraNPCDialogArray(extraNPCDialogue[] dialogues, string type,
            out List<object> metadata)
        {
            var result = new List<LocalizeString>();
            metadata = new List<object>();
            if (dialogues == null) return result;
            for (int i = 0; i < dialogues.Length; i++)
            {
                var dialogue = dialogues[i];
                if (dialogue.dialogueScript.Length == 0) return result;

                for (int j = 0; j < dialogue.dialogueScript.Length; j++)
                {
                    var dialogueScript = dialogue.dialogueScript[j];
                    if (dialogueScript.TextByLanguage.Length == 0) continue;
                    result.Add(GetLocalizeStringFromStringArray(dialogueScript.TextByLanguage));
                    metadata.Add(new LocalizeData
                    {
                        Type = type,
                        dialogueIndex = i,
                        scriptIndex = j
                    });
                }
            }

            return result;
        }

        private List<LocalizeString> GetLocalStringFromDialogArray(Dialogue[] dialogues, string type,
            out List<object> metadata)
        {
            var result = new List<LocalizeString>();
            metadata = new List<object>();
            if (dialogues == null) return result;
            for (int i = 0; i < dialogues.Length; i++)
            {
                var dialogue = dialogues[i];
                if (dialogue.TextByLanguage.Length == 0) continue;
                result.Add(GetLocalizeStringFromStringArray(dialogue.TextByLanguage));
                metadata.Add(new LocalizeData
                {
                    Type = type,
                    dialogueIndex = i,
                });
            }

            return result;
        }


        protected override void UpdateData(extraNPC asset, object columnKey, object value, object metadata)
        {
            var data = ConvertTo<LocalizeData>(metadata);
            var index = GetLocalizeIndexFromColumnKey(columnKey);

            switch (data.Type)
            {
                case "NPCDialogue":
                    UpdateExtraDialogueArray(asset.NPCDialogue);
                    break;
                case "requestDialogue":
                    UpdateExtraDialogueArray(asset.requestDialogue);
                    break;
                case "requestUnDoneDialogue":
                    UpdateExtraDialogueArray(asset.requestUnDoneDialogue);
                    break;
                case "requestDoneDialogue":
                    UpdateDialogueArray(asset.requestDoneDialogue);
                    break;
                case "questRequirementRejectedDialogue":
                    UpdateDialogueArray(asset.questRequirementRejectedDialogue);
                    break;
                case "AfterRequestDialogue": UpdateExtraDialogueArray(asset.AfterRequestDialogue); break;
                case "NPCDialogueCondition": UpdateExtraDialogueArray(asset.NPCDialogueCondition); break;
                case "NPCDialogueEvent": UpdateExtraDialogueArray(asset.NPCDialogueEvent); break;
                case "giveItemNPCDialogue": UpdateExtraDialogueArray(asset.giveItemNPCDialogue); break;
                case "itemGivenNPCDialogue": UpdateExtraDialogueArray(asset.itemGivenNPCDialogue); break;
                case "wifeNabila": UpdateExtraDialogueArray(asset.wifeNabila); break;
                case "wifeIsma": UpdateExtraDialogueArray(asset.wifeIsma); break;
                case "wifeSarah": UpdateExtraDialogueArray(asset.wifeSarah); break;
                case "wifeWindy": UpdateExtraDialogueArray(asset.wifeWindy); break;
                case "wifeCitra": UpdateExtraDialogueArray(asset.wifeCitra); break;
                case "wifeJessica": UpdateExtraDialogueArray(asset.wifeJessica); break;
                case "wifeTasya": UpdateExtraDialogueArray(asset.wifeTasya); break;
                case "wifeIndah": UpdateExtraDialogueArray(asset.wifeIndah); break;
                case "wifeNissa": UpdateExtraDialogueArray(asset.wifeNissa); break;
                case "wifeAsih": UpdateExtraDialogueArray(asset.wifeAsih); break;
                case "wifeMilah": UpdateExtraDialogueArray(asset.wifeMilah); break;
                case "wifeImas": UpdateExtraDialogueArray(asset.wifeImas); break;
            }
            return;

            void UpdateExtraDialogueArray(extraNPCDialogue[] dialogue)
            {
                if (data.dialogueIndex >= dialogue.Length) return;
                var target = dialogue[data.dialogueIndex];
                if (data.scriptIndex >= target.dialogueScript.Length) return;
                var script = target.dialogueScript[data.scriptIndex];
                script.TextByLanguage[index] = $"{value}";
            }

            void UpdateDialogueArray(Dialogue[] dialogue)
            {
                if (data.dialogueIndex >= dialogue.Length) return;
                var target = dialogue[data.dialogueIndex];
                target.TextByLanguage[index] = $"{value}";
            }
        }

        private struct LocalizeData
        {
            public string Type;
            public int dialogueIndex;
            public int scriptIndex;
        }
    }
}