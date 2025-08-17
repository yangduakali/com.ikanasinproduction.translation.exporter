using System.Collections.Generic;

namespace citampi.TranslationExporter
{
    public class NPCDataProcessor : ResourcesJsonExporterScriptable<NPCData>
    {
        public override string GroupId => "NPCData";
        protected override string ResourcesPath => "NPCData";

        protected override LocalizeString[] GetLocalString(NPCData asset, out List<object> metadata)
        {
            var result = new List<LocalizeString>();
            metadata = new List<object>();
            var tempMetadata = new List<object>();

            AddFromSingleArray(asset.name, "name");
            AddFromSingleArray(asset.completeName, "completeName");
            AddFromSingleArray(asset.job, "job");
            AddFromSingleArray(asset.hobby, "hobby");

            AddFromDialogue(asset.reactionBenci, "reactionBenci");
            AddFromDialogue(asset.reactionBiasa, "reactionBiasa");
            AddFromDialogue(asset.reactionSuka, "reactionSuka");
            AddFromDialogue(asset.reactionFavorit, "reactionFavorit");
            AddFromDialogue(asset.reactionSpesial, "reactionSpesial");

            AddFromDialogue(asset.NPCOptionDialogue, "NPCOptionDialogue");

            AddFromDialogue(asset.jobQuestDialogue, "jobQuestDialogue");
            AddFromDialogue(asset.jobQuestFinishDialogue, "jobQuestFinishDialogue");
            AddFromDialogue(asset.fishQuestDialogue, "fishQuestDialogue");
            AddFromDialogue(asset.fishQuestFinishDialogue, "fishQuestFinishDialogue");
            AddFromDialogue(asset.itemQuestDialogue, "itemQuestDialogue");
            AddFromDialogue(asset.itemQuestFinishDialogue, "itemQuestFinishDialogue");

            if (asset.isWifeCandidate)
            {
                AddFromDialogue(asset.WifeFishQuestDialogue, "WifeFishQuestDialogue");
                AddFromDialogue(asset.WifeFishQuestFinishDialogue, "WifeFishQuestFinishDialogue");
                AddFromDialogue(asset.WifeItemQuestDialogue, "WifeItemQuestDialogue");
                AddFromDialogue(asset.WifeItemQuestFinishDialogue, "WifeItemQuestFinishDialogue");
                AddFromDialogue(asset.ngidamFishDialogue01, "ngidamFishDialogue01");
                AddFromDialogue(asset.ngidamVeggieDialogue01, "ngidamVeggieDialogue01");
            }

            metadata.AddRange(tempMetadata);
            return result.ToArray();

            void AddFromSingleArray(string[] texts, string type)
            {
                if (texts == null || texts.Length == 0) return;
                var localizeStrings = GetLocalizeStringFromStringArray(texts, asset);
                result.Add(localizeStrings);
                tempMetadata.Add(new LocalizeData()
                {
                    Type = type
                });
            }

            void AddFromDialogue(Dialogue dialogue, string type)
            {
                if (dialogue.TextByLanguage == null || dialogue.TextByLanguage.Length == 0) return;
                var localizeStrings = GetLocalizeStringFromStringArray(dialogue.TextByLanguage, asset);
                result.Add(localizeStrings);
                tempMetadata.Add(new LocalizeData()
                {
                    Type = type
                });
            }
        }

        protected override void UpdateData(NPCData asset, object columnKey, object value, object metadata)
        {
            var data = ConvertTo<LocalizeData>(metadata);
            var index = GetLocalizeIndexFromColumnKey(columnKey);
            switch (data.Type)
            {
                case "name" : AddFromSingleArray(asset.name); break;
                case "completeName" : AddFromSingleArray(asset.completeName); break;
                case "job" : AddFromSingleArray(asset.job); break;
                case "hobby" : AddFromSingleArray(asset.hobby); break;
                case "reactionBenci" : AddFromDialogue(asset.reactionBenci); break;
                case "reactionBiasa" : AddFromDialogue(asset.reactionBiasa); break;
                case "reactionSuka" : AddFromDialogue(asset.reactionSuka); break;
                case "reactionFavorit" : AddFromDialogue(asset.reactionFavorit); break;
                case "reactionSpesial" : AddFromDialogue(asset.reactionSpesial); break;
                case "NPCOptionDialogue" : AddFromDialogue(asset.NPCOptionDialogue); break;
                case "jobQuestDialogue" : AddFromDialogue(asset.jobQuestDialogue); break;
                case "jobQuestFinishDialogue" : AddFromDialogue(asset.jobQuestFinishDialogue); break;
                case "fishQuestDialogue" : AddFromDialogue(asset.fishQuestDialogue); break;
                case "fishQuestFinishDialogue" : AddFromDialogue(asset.fishQuestFinishDialogue); break;
                case "itemQuestDialogue" : AddFromDialogue(asset.itemQuestDialogue); break;
                case "itemQuestFinishDialogue" : AddFromDialogue(asset.itemQuestFinishDialogue); break;
                case "WifeFishQuestDialogue" : AddFromDialogue(asset.WifeFishQuestDialogue); break;
                case "WifeFishQuestFinishDialogue" : AddFromDialogue(asset.WifeFishQuestFinishDialogue); break;
                case "WifeItemQuestDialogue" : AddFromDialogue(asset.WifeItemQuestDialogue); break;
                case "WifeItemQuestFinishDialogue" : AddFromDialogue(asset.WifeItemQuestFinishDialogue); break;
                case "ngidamFishDialogue01" : AddFromDialogue(asset.ngidamFishDialogue01); break;
                case "ngidamVeggieDialogue01" : AddFromDialogue(asset.ngidamVeggieDialogue01); break;
                
            }

            return;
            void AddFromSingleArray(string[] texts)
            {
                if (texts == null || texts.Length == 0) return;
                texts[index] = $"{value}";
            }

            void AddFromDialogue(Dialogue dialogue)
            {
                if (dialogue.TextByLanguage == null || dialogue.TextByLanguage.Length == 0) return;
                dialogue.TextByLanguage[index] = $"{value}";
            }
        }

        private struct LocalizeData
        {
            public string Type;
        }
    }
}