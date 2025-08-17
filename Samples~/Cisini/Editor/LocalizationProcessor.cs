using ikanAsin.localization;
using ikanAsin.localization.editor;
using ikanAsin.util.editor;
using System;
using System.Collections.Generic;
using TranslationExporter;
using UnityEditor;

namespace cisini.TranslationExporter
{
    public class LocalizationProcessor : CisiniExporterProcessor
    {
        public override string GroupId => "Localization";
        private string Path => LocalizationEditor.ResourceStringFolder;
        private readonly Dictionary<string, Dictionary<Localize, LocaleStringGroup>> _groupByName = new();
        private int LocalizeIdCount => Enum.GetValues(typeof(Localize)).Length;

        protected override List<ILocalizeString> GetLocalizeData()
        {
            var result = new List<ILocalizeString>();
            PopulateGroups();
            foreach (var (groupName, groups) in _groupByName)
            {
                var ids = groups[0].source.Keys;
                foreach (var id in ids)
                {
                    var localizeString = new LocalizeString();
                    var isValid = true;
                    for (int i = 0; i < LocalizeIdCount; i++)
                    {
                        var localizeType = (Localize)i;
                        var group = groups[localizeType];
                        if (!group.source.TryGetValue(id, out var value))
                        {
                            isValid = false;
                            break;
                        }
                        localizeString.SetValue(localizeType, value);
                    }
                    if (!isValid) continue;
                    localizeString.Key = $"{groupName}_{id}";
                    result.Add(localizeString);
                }
            }
            return result;
        }

        
        protected override void UpdateValue(object rowKey, object columnKey, object value)
        {
            var splitKey = $"{rowKey}".Split('_');
            if (splitKey.Length < 2) return;
            var groupName = splitKey[0];
            if (!int.TryParse(splitKey[1], out var id)) return;
            var localizeType = (Localize)GetLocalizeIndexFromColumnKey(columnKey);
            if (!_groupByName.TryGetValue(groupName, out var localizeGroups)) return;
            if (!localizeGroups.TryGetValue(localizeType, out var group)) return;
            if (!group.source.ContainsKey(id)) return;
            group.source[id] = $"{value}";
            EditorUtility.SetDirty(group);
        }

        private void PopulateGroups()
        {
            _groupByName.Clear();
            var groups = EditorExt.LoadAllScriptableObject<LocaleStringGroup>(Path);
            foreach (var group in groups)
            {
                var groupName = group.GroupName();
                if (!_groupByName.ContainsKey(groupName))
                {
                    _groupByName.Add(groupName, new());
                }

                var localeType = group.Target();
                _groupByName[groupName][localeType] = group;
            }
        }
    }
}