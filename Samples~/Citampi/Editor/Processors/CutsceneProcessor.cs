using System.Collections.Generic;

namespace citampi.TranslationExporter
{
    public class CutsceneProcessor : ResourcesJsonExporterScriptable<CutScene>
    {
        public override string GroupId => "Cutscene";
        protected override string ResourcesPath => "Cutscene";

        protected override LocalizeString[] GetLocalString(CutScene asset, out List<object> metadata)
        {
            var result = new List<LocalizeString>();
            metadata = new List<object>();

            if (asset.statusDescription.Length != 0)
            {
                result.Add(GetLocalizeStringFromStringArray(asset.statusDescription, asset));
                metadata.Add(new LocalizeData()
                {
                    Type = "Status"
                });
            }

            if (asset.cutsceneInfo.Length != 0)
            {
                result.Add(GetLocalizeStringFromStringArray(asset.cutsceneInfo, asset));
                metadata.Add(new LocalizeData()
                {
                    Type = "Info"
                });
            }

            for (var i = 0; i < asset.cutsceneList.Count; i++)
            {
                var item = asset.cutsceneList[i];
                switch (item.ItemFunction)
                {
                    case CutsceneEnum.StartAConversation:
                    {
                        for (var j = 0; j < item.dialogues.Length; j++)
                        {
                            var dialogue = item.dialogues[j];
                            result.Add(GetLocalizeStringFromStringArray(dialogue.TextByLanguage, asset));
                            metadata.Add(new LocalizeData()
                            {
                                Type = item.ItemFunction.ToString(),
                                IndexItem = i,
                            });
                        }

                        break;
                    }
                    case CutsceneEnum.giveAChoice:
                        result.Add(GetLocalizeStringFromStringArray(item.ChoiceDialogue.TextByLanguage, asset));
                        metadata.Add(new LocalizeData()
                        {
                            Type = item.ItemFunction.ToString(),
                            IndexItem = i,
                        });
                        break;
                }
            }

            return result.ToArray();
        }

        protected override void UpdateData(CutScene asset, object columnKey, object value, object metadata)
        {
            var data = ConvertTo<LocalizeData>(metadata);
            var index = GetLocalizeIndexFromColumnKey(columnKey);
            if (data.Type == CutsceneEnum.StartAConversation.ToString())
            {
                if (data.IndexItem >= asset.cutsceneList.Count) return;
                var item = asset.cutsceneList[data.IndexItem];

                if (item.ItemFunction != CutsceneEnum.StartAConversation) return;
                if (data.IndexDialogue >= item.dialogues.Length) return;
                var dialogue = item.dialogues[data.IndexDialogue];
                dialogue.TextByLanguage[index] = $"{value}";
                return;
            }

            if (data.Type == CutsceneEnum.giveAChoice.ToString())
            {
                if (data.IndexItem >= asset.cutsceneList.Count) return;
                var item = asset.cutsceneList[data.IndexItem];
                if (item.ItemFunction != CutsceneEnum.giveAChoice) return;
                item.ChoiceDialogue.TextByLanguage[index] = $"{value}";
                return;
            }

            switch (data.Type)
            {
                case "Status":
                    asset.statusDescription[index] = $"{value}";
                    break;
                case "Info":
                    asset.cutsceneInfo[index] = $"{value}";
                    break;
            }
        }

        private struct LocalizeData
        {
            public string Type;
            public int IndexItem;
            public int IndexDialogue;
        }
    }
}