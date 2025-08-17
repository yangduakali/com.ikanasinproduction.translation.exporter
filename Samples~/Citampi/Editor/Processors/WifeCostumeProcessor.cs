using System.Collections.Generic;

namespace citampi.TranslationExporter
{
    public class WifeCostumeProcessor : ResourcesJsonExporterScriptable<wifeCostume>
    {
        public override string GroupId => "WFCostume";
        protected override string ResourcesPath => "wifeCostume";
        protected override LocalizeString[] GetLocalString(wifeCostume asset, out List<object> metadata)
        {
            var result = new List<LocalizeString>();
            metadata = new List<object>();
            result.Add(GetLocalizeStringFromStringArray(asset.CostumeName, asset));
            metadata.Add("");
            return result.ToArray();
        }
        protected override void UpdateData(wifeCostume asset, object columnKey, object value, object metadata)
        {
            asset.CostumeName[GetLocalizeIndexFromColumnKey(columnKey)] = $"{value}";
        }
    }
}