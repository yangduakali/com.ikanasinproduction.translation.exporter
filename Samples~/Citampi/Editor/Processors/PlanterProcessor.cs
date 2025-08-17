using System.Collections.Generic;

namespace citampi.TranslationExporter
{
    public class PlanterProcessor : ResourcesJsonExporterScriptable<planter>
    {
        public override string GroupId => "Plantr";
        protected override string ResourcesPath => "Planter";
        protected override LocalizeString[] GetLocalString(planter asset, out List<object> metadata)
        {
            var result = new List<LocalizeString>();
            metadata = new List<object>();
            result.Add(GetLocalizeStringFromStringArray(asset.potName, asset));
            metadata.Add("");
            return result.ToArray();

        }
        protected override void UpdateData(planter asset, object columnKey, object value, object metadata)
        {
            var index = GetLocalizeIndexFromColumnKey(columnKey);
            asset.potName[index] = $"{value}";
        }
    }
}