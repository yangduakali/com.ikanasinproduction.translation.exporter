using System.Collections.Generic;

namespace citampi.TranslationExporter
{
    public class InventoryApartmentProcessor : ResourcesJsonExporterScriptable<ApartmentInventoryBase>
    {
        public override string GroupId => "AprItem";
        protected override string ResourcesPath => "InventoryApartment";
        
        protected override LocalizeString[] GetLocalString(ApartmentInventoryBase asset, out List<object> metadata)
        {
            var result = new List<LocalizeString>();
            metadata = new List<object>();
            result.Add(GetLocalizeStringFromStringArray(asset.itemName, asset));
            metadata.Add(new LocalizeData
            {
                Type = "Name"
            });
            result.Add(GetLocalizeStringFromStringArray(asset.descriptionArray, asset));
            metadata.Add(new LocalizeData
            {
                Type = "Desc"
            });
            return result.ToArray();
            
            
        }
        
        protected override void UpdateData(ApartmentInventoryBase asset, object columnKey, object value, object metadata)
        {
            var data = ConvertTo<LocalizeData>(metadata);
            var index = GetLocalizeIndexFromColumnKey(columnKey);
            switch (data.Type)
            {
                case "Name":
                    asset.itemName[index] = $"{value}";
                    break;
                case "Desc":
                    asset.descriptionArray[index] = $"{value}";
                    break;
            }
        }
        
        private struct LocalizeData 
        {
            public string Type;
        }
    }
}