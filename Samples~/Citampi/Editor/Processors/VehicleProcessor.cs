using System.Collections.Generic;

namespace citampi.TranslationExporter
{
    public class VehicleProcessor : ResourcesJsonExporterScriptable<vehicle>
    {
        public override string GroupId => "Vehicle";
        protected override string ResourcesPath => "Vehicles";
        protected override LocalizeString[] GetLocalString(vehicle asset, out List<object> metadata)
        {
            var result = new List<LocalizeString>();
            metadata = new List<object>();
            if (asset.VehicleName.Length != 0)
            {
                result.Add(GetLocalizeStringFromStringArray(asset.VehicleName, asset));
                metadata.Add(new LocalizeData
                {
                    Type = "VehicleName"
                });
            }     
            
            if (asset.description.Length != 0)
            {
                result.Add(GetLocalizeStringFromStringArray(asset.description, asset));
                metadata.Add(new LocalizeData
                {
                    Type = "description"
                });
            }     


            return result.ToArray();
        }
        protected override void UpdateData(vehicle asset, object columnKey, object value, object metadata)
        {
            var data = ConvertTo<LocalizeData>(metadata);
            var index = GetLocalizeIndexFromColumnKey(columnKey);

            if (data.Type == "VehicleName")
            {
                asset.VehicleName[index] = $"{value}";
            }
            
            if (data.Type == "description")
            {
                asset.description[index] = $"{value}";
            }
        }
        
        private struct LocalizeData
        {
            public string Type;
        }
    }
}