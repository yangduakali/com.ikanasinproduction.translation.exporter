using System.Collections.Generic;

namespace citampi.TranslationExporter
{
    public class InventoryItemProcessor : ResourcesJsonExporterScriptable<InventoryItemBase>
    {
        public override string GroupId => "InvItem";
        protected override string ResourcesPath => "InventoryItem";
        
        protected override LocalizeString[] GetLocalString(InventoryItemBase asset, out List<object> metadata)
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
        
        protected override void UpdateData(InventoryItemBase asset, object columnKey, object value, object metadata)
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