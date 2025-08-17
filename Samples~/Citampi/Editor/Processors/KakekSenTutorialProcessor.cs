using System.Collections.Generic;

namespace citampi.TranslationExporter
{
    public class KakekSenTutorialProcessor: ResourcesJsonExporterScriptable<kakekSenTutorial>
    {
        public override string GroupId => "SenTutorial";
        protected override string ResourcesPath => "tutorialDialogue";
        protected override LocalizeString[] GetLocalString(kakekSenTutorial asset, out List<object> metadata)
        {
            var result = new List<LocalizeString>();
            metadata = new List<object>();
            if(asset.characterDialogues.Length == 0) return result.ToArray();

            for (var i = 0; i < asset.characterDialogues.Length; i++)
            {
                var dialogue = asset.characterDialogues[i];
                if(dialogue.TextByLanguage.Length == 0) continue;
                result.Add(GetLocalizeStringFromStringArray(dialogue.TextByLanguage, asset));
                metadata.Add(new LocalizeData
                {
                    dialogueIndex = i,
                });

            }

            return result.ToArray();
        }
        
        protected override void UpdateData(kakekSenTutorial asset, object columnKey, object value, object metadata)
        {
            var data = ConvertTo<LocalizeData>(metadata);
            var index = GetLocalizeIndexFromColumnKey(columnKey);
            if(data.dialogueIndex >= asset.characterDialogues.Length) return;
            asset.characterDialogues[index].TextByLanguage[index] = $"{value}";
        }
        
        private struct LocalizeData
        {
            public int dialogueIndex;
        }
    }
}