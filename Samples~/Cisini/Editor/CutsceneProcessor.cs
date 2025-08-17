using cutscene.me;
using ikanAsin.util.editor;
using System.Collections.Generic;
using TranslationExporter;
using UnityEditor;

namespace cisini.TranslationExporter
{
    public class CutsceneProcessor : CisiniExporterProcessor
    {
        public override string GroupId => "Cutscene";

        private readonly Dictionary<int, CutsceneGraph> _graphs = new();

        protected override List<ILocalizeString> GetLocalizeData()
        {
            _graphs.Clear();
            var result = new List<ILocalizeString>();
            var assets = EditorExt.LoadAllScriptableObject<CutsceneGraph>();
            foreach (var graph in assets)
            {
                _graphs.Add(graph.ID, graph);
                foreach (var node in graph.nodes)
                {
                    switch (node)
                    {
                        case DialogText dialogText:
                            result.AddRange(FromDialogText(dialogText, graph));
                            break;
                        case Narration narration:
                            var localizeString = GetLocalizeStringFromStringArray(narration.texts, graph);
                            localizeString.Key = $"N_{graph.ID}_{node.id}";
                            result.Add(localizeString);
                        break;
                    }
                }
            }

            return result;
        }

        private  List<LocalizeString> FromDialogText(DialogText dialogText, CutsceneGraph graph)
        {
            var result = new List<LocalizeString>();
            var mainLocalizeString = GetLocalizeStringFromStringArray(dialogText.texts, graph);
            mainLocalizeString.Key = $"T_{graph.ID}_{dialogText.id}";
            result.Add(mainLocalizeString);
            if(dialogText is not DialogChoice dialogChoice) return result;

            for (int i = 0; i < dialogChoice.choiceEntry.Count; i++)
            {
                var choiceEntry = dialogChoice.choiceEntry[i];
                var choiceString = GetLocalizeStringFromStringArray(choiceEntry.texts, graph);
                choiceString.Key = $"C_{graph.ID}_{dialogChoice.id}_{i}";
                result.Add(choiceString);
            }
            return result;
        }

        protected override void UpdateValue(object rowKey, object columnKey, object value)
        {
            var splitKey = $"{rowKey}".Split('_');
            if(splitKey.Length < 3) return;
            var prefix = splitKey[0];
            if (!int.TryParse(splitKey[1], out var assetId)) return;
            if (!int.TryParse(splitKey[2], out var nodeId)) return;
            if (!_graphs.TryGetValue(assetId, out var graph)) return;
            var node = graph.nodes.Find(x => x.id == nodeId);
            if(node is null) return;
            var index = GetLocalizeIndexFromColumnKey(columnKey);

            switch (prefix)
            {
                case "T":
                    if (node is not DialogText dialogText) break;
                    dialogText.texts[index] = $"{value}";
                    break;
                case "C":
                    if (node is not DialogChoice dialogChoice) break;
                    if(splitKey.Length < 4) break ;
                    if (!int.TryParse(splitKey[3], out var entryIndex)) break;
                    if(entryIndex >= dialogChoice.choiceEntry.Count) break;
                    var choiceEntry = dialogChoice.choiceEntry[entryIndex];
                    choiceEntry.texts[index] = $"{value}";
                    break;
                case "N":
                    if (node is not Narration narration) break;
                    narration.texts[index] = $"{value}";
                    break;
            }
            
            EditorUtility.SetDirty(node);
            EditorUtility.SetDirty(graph);
        }
    }
}