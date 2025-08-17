using System.Collections.Generic;
using MiniGame.HiddenObject;

namespace citampi.TranslationExporter
{
    public class HOItemProcessor : ResourcesJsonExporterScriptable<HO_InventoryItem>
    {
        public override string GroupId => "MiniGame";
        protected override string ResourcesPath => "Minigame";

        protected override LocalizeString[] GetLocalString(HO_InventoryItem asset, out List<object> metadata)
        {
            var result = new List<LocalizeString>();
            metadata = new List<object>();


            result.Add(GetLocalizeStringFromStringArray(asset.itemName, asset));
            metadata.Add(new LocalizeData
            {
                Type = "Name"
            });
 
            if (asset.descriptionArray.Length != 0)
            {
                result.Add(GetLocalizeStringFromStringArray(asset.descriptionArray, asset));
                metadata.Add(new LocalizeData
                {
                    Type = "Desc"
                });
            }

            return result.ToArray();
        }

        protected override void UpdateData(HO_InventoryItem asset, object columnKey, object value, object metadata)
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