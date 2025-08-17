using ikanAsin.rpgGame.dialogue;
using ikanAsin.util.editor;
using System.Collections.Generic;
using System.Linq;
using TranslationExporter;
using UnityEditor;
using UnityEngine;

namespace cisini.TranslationExporter
{
    public abstract class DialogueBaseProcessor : CisiniExporterProcessor
    {
        private readonly Dictionary<int, Dictionary<int, DialogNode>> _graphNodes = new();

        protected override List<ILocalizeString> GetLocalizeData()
        {
            var result = new List<ILocalizeString>();
            var assets = EditorExt.LoadAllScriptableObject<DialogGraphBase>();
            _graphNodes.Clear();
            foreach (var graph in assets)
            {
                if (!IsValidGraph(graph)) continue;
                var dialogNodes = FlattenNodes(graph);
                _graphNodes.Add(graph.ID, dialogNodes.ToDictionary(x => x.id));
                var localize = GetLocalizeDataFromListNode(dialogNodes, $"{graph.ID}");
                localize.ForEach(x => x.Asset = graph);
                result.AddRange(localize);
            }

            return result;
        }

        private List<DialogNode> FlattenNodes(DialogGraphBase graph)
        {
            var result = new List<DialogNode>();
            if (graph is null) return result;
            if (graph.nodes is null) return result;
            foreach (var node in graph.nodes)
            {
                if (node is null) continue;
                if (node is NestNode nestNode)
                {
                    if (nestNode.graph is null) continue;
                    result.AddRange(FlattenNodes(nestNode.graph));
                    continue;
                }

                if (!IsLocalizeNode(node)) continue;
                result.Add(node);
            }

            return result;
        }

        private bool IsLocalizeNode(DialogNode node)
        {
            return node is TextNode or ChoiceNode or RandomNode ;
        }

        protected override void UpdateValue(object rowKey, object columnKey, object value)
        {
            var splitKey = $"{rowKey}".Split('_');
            if (splitKey.Length < 3) return;
            var prefix = splitKey[0];
            if (!int.TryParse(splitKey[1], out var assetId)) return;
            if (!int.TryParse(splitKey[2], out var nodeId)) return;
            if (!_graphNodes.TryGetValue(assetId, out var nodes)) return;
            if (!nodes.TryGetValue(nodeId, out var node)) return;
            if (node is null) return;
            var index = GetLocalizeIndexFromColumnKey(columnKey);

            switch (prefix)
            {
                case "T":
                    if (node is TextNode dialogText) dialogText.texts[index] = $"{value}";
                    break;
                case "C":
                    if (node is not ChoiceNode dialogChoice) break;
                    if (splitKey.Length == 3)
                    {
                        dialogChoice.textEntry[index] = $"{value}";
                        break;
                    }

                    if (splitKey.Length == 4)
                    {
                        if (!int.TryParse(splitKey[3], out var entryIndex)) break;
                        if (entryIndex >= dialogChoice.choiceEntry.Count) break;
                        var choiceEntry = dialogChoice.choiceEntry[entryIndex];
                        choiceEntry.texts[index] = $"{value}";
                    }

                    break;
                case "R":
                    if (node is not RandomNode randomNode) break;
                    if (splitKey.Length == 4)
                    {
                        if (!int.TryParse(splitKey[3], out var entryIndex)) break;
                        if (entryIndex >= randomNode.entries.Count) break;
                        var entry = randomNode.entries[entryIndex];
                        entry.texts[index] = $"{value}";
                    }

                    break;
            }

            EditorUtility.SetDirty(node);
        }

        protected List<LocalizeString> GetLocalizeDataFromListNode(List<DialogNode> nodes, string graphId)
        {
            var result = new List<LocalizeString>();
            foreach (var node in nodes)
            {
                switch (node)
                {
                    case TextNode textNode:
                        result.Add(GetLocalizeDataFromTextNode(textNode, $"{graphId}"));
                        break;
                    case ChoiceNode choiceNode:
                        result.AddRange(GetLocalizeDataFromChoiceNode(choiceNode, $"{graphId}"));
                        break;
                    case RandomNode randomNode:
                        for (int i = 0; i < randomNode.entries.Count; i++)
                        {
                            RandomNode.Entry entry = randomNode.entries[i];
                            var localizeString = GetLocalizeStringFromStringArray(entry.texts);
                            localizeString.Key = $"R_{graphId}_{node.id}_{i}";
                            result.Add(localizeString);
                        }
                        break;
                }
            }

            return result;
        }

        protected LocalizeString GetLocalizeDataFromTextNode(TextNode node, string graphId)
        {
            var localizeString = GetLocalizeStringFromStringArray(node.texts);
            localizeString.Key = $"T_{graphId}_{node.id}";
            return localizeString;
        }

        protected List<LocalizeString> GetLocalizeDataFromChoiceNode(ChoiceNode node, string graphId)
        {
            var result = new List<LocalizeString>();
            var mainText = GetLocalizeStringFromStringArray(node.textEntry);
            mainText.Key = $"C_{graphId}_{node.id}";
            result.Add(mainText);

            for (int i = 0; i < node.choiceEntry.Count; i++)
            {
                var choiceEntry = node.choiceEntry[i];
                var choiceString = GetLocalizeStringFromStringArray(choiceEntry.texts);
                choiceString.Key = $"C_{graphId}_{node.id}_{i}";
                result.Add(choiceString);
            }

            return result;
        }

        protected abstract bool IsValidGraph(DialogGraphBase graph);
    }

    public class DialogueProcessor : DialogueBaseProcessor
    {
        public override string GroupId => "Dialogue";

        protected override bool IsValidGraph(DialogGraphBase graph)
        {
            return graph is not ChatGraph;
        }
    }

    public class ChatProcessor : DialogueBaseProcessor
    {
        public override string GroupId => "Chat";

        protected override bool IsValidGraph(DialogGraphBase graph)
        {
            return graph is ChatGraph;
        }
    }
}