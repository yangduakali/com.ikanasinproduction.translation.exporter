using System.Collections.Generic;

namespace citampi.TranslationExporter
{
    public class AbilityProcessor : ResourcesJsonExporterScriptable<ability>
    {
        public override string GroupId => "Ability";
        protected override string ResourcesPath => "Ability";
        
        protected override LocalizeString[] GetLocalString(ability asset, out List<object> metadata)
        {
            var result = new List<LocalizeString>();
            metadata = new List<object>();
            result.Add(GetLocalizeStringFromStringArray(asset.names, asset));
            metadata.Add(new LocalizeData
            {
                Type = "Name"
            });
            result.Add(GetLocalizeStringFromStringArray(asset.deskripsi, asset));
            metadata.Add(new LocalizeData
            {
                Type = "Desc"
            });
            return result.ToArray();
        }
        
        protected override void UpdateData(ability asset, object columnKey, object value, object metadata)
        {
            var data = ConvertTo<LocalizeData>(metadata);
            var index = GetLocalizeIndexFromColumnKey(columnKey);
            switch (data.Type)
            {
                case "Name":
                    asset.names[index] = $"{value}";
                    break;
                case "Desc":
                    asset.deskripsi[index] = $"{value}";
                    break;
            }
        }

        private struct LocalizeData 
        {
            public string Type;
        }
    }
}
