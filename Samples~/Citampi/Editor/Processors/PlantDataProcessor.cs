using System.Collections.Generic;

namespace citampi.TranslationExporter
{
    public class PlantDataProcessor : ResourcesJsonExporterScriptable<plantData>
    {
        public override string GroupId => "Plant";
        protected override string ResourcesPath => "Plant";
        protected override LocalizeString[] GetLocalString(plantData asset, out List<object> metadata)
        {
            var result = new List<LocalizeString>();
            metadata = new List<object>();
            result.Add(GetLocalizeStringFromStringArray(asset.plantName, asset));
            metadata.Add("");
            return result.ToArray();
        }
        protected override void UpdateData(plantData asset, object columnKey, object value, object metadata)
        {
            var index = GetLocalizeIndexFromColumnKey(columnKey);
            asset.plantName[index] = $"{value}";
        }
    }
}