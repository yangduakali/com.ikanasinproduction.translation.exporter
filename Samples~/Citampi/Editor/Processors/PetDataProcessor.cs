using System.Collections.Generic;

namespace citampi.TranslationExporter
{
    public class PetDataProcessor : ResourcesJsonExporterScriptable<petData>
    {
        public override string GroupId => "Pet";
        protected override string ResourcesPath => "petData";
        protected override LocalizeString[] GetLocalString(petData asset, out List<object> metadata)
        {
            var result = new List<LocalizeString>();
            metadata = new List<object>();
            var tempMetadata = new List<object>();
            
            AddFromSingleArray(asset.originalPetName, "originalPetName");
            AddFromDialogue(asset.NPCOptionDialogue,"NPCOptionDialogue");
            AddFromDialogue(asset.reactionBenci,"reactionBenci");
            AddFromDialogue(asset.reactionBiasa,"reactionBiasa");
            AddFromDialogue(asset.reactionSuka,"reactionSuka");
            AddFromDialogue(asset.reactionFavorit,"reactionFavorit");
            AddFromDialogue(asset.reactionSpesial,"reactionSpesial");
            AddFromDialogue(asset.dialogLiar,"dialogLiar");
            AddFromDialogue(asset.dialogKenal,"dialogKenal");
            AddFromDialogue(asset.dialogTrusting,"dialogTrusting");
            AddFromDialogue(asset.dialogOwned,"dialogOwned");

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
        protected override void UpdateData(petData asset, object columnKey, object value, object metadata)
        {
                        var data = ConvertTo<LocalizeData>(metadata);
            var index = GetLocalizeIndexFromColumnKey(columnKey);
            switch (data.Type)
            {
                case "originalPetName" : AddFromSingleArray(asset.originalPetName); break;
                case "NPCOptionDialogue" : AddFromDialogue(asset.NPCOptionDialogue); break;
                case "reactionBenci" : AddFromDialogue(asset.reactionBenci); break;
                case "reactionBiasa" : AddFromDialogue(asset.reactionBiasa); break;
                case "reactionSuka" : AddFromDialogue(asset.reactionSuka); break;
                case "reactionFavorit" : AddFromDialogue(asset.reactionFavorit); break;
                case "reactionSpesial" : AddFromDialogue(asset.reactionSpesial); break;
                case "dialogLiar" : AddFromDialogue(asset.dialogLiar); break;
                case "dialogKenal" : AddFromDialogue(asset.dialogKenal); break;
                case "dialogTrusting" : AddFromDialogue(asset.dialogTrusting); break;
                case "dialogOwned" : AddFromDialogue(asset.dialogOwned); break;
                
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