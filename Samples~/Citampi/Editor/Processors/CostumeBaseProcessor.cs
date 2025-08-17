using System.Collections.Generic;

namespace citampi.TranslationExporter
{
    public class CostumeBaseProcessor : ResourcesJsonExporterScriptable<costumeBase>
    {
        public override string GroupId => "Costume";
        protected override string ResourcesPath => "Costumes";
        protected override void UpdateData(costumeBase asset, object columnKey, object value, object metadata)
        {
            asset.CostumeName[GetLocalizeIndexFromColumnKey(columnKey)] = $"{value}";
        }
        protected override LocalizeString[] GetLocalString(costumeBase asset, out List<object> metadata)
        {
            var result = new List<LocalizeString>();
            metadata = new List<object>();
            result.Add(GetLocalizeStringFromStringArray(asset.CostumeName, asset));
            metadata.Add("");
            return result.ToArray();
        }
    }
}