using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;

namespace TranslationExporter
{
    public class TranslationExporterWindow : EditorWindow
    {
        private TextAsset ServiceAccountJson
        {
            get
            {
                if (_serviceAccountJson != null) return _serviceAccountJson;
                var cachePath =
                    EditorPrefs.GetString($"{GetType().FullName}_{nameof(ServiceAccountJson)}", "");
                _serviceAccountJson = AssetDatabase.LoadAssetAtPath<TextAsset>(cachePath);
                return _serviceAccountJson;
            }
            set
            {
                var path = value == null ? "" : AssetDatabase.GetAssetPath(value);
                EditorPrefs.SetString($"{GetType().FullName}_{nameof(ServiceAccountJson)}", path);
                _serviceAccountJson = value;
            }
        }

        private string GoogleSheetAppName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_googleSheetAppName)) return _googleSheetAppName;
                return EditorPrefs.GetString($"{GetType().FullName}_{nameof(GoogleSheetAppName)}", "");
            }
            set
            {
                _googleSheetAppName = value;
                EditorPrefs.SetString($"{GetType().FullName}_{nameof(GoogleSheetAppName)}", value);
            }
        }

        private string SheetId
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_sheetId)) return _sheetId;
                return EditorPrefs.GetString($"{GetType().FullName}_{nameof(SheetId)}", "");
            }
            set
            {
                _sheetId = value;
                EditorPrefs.SetString($"{GetType().FullName}_{nameof(SheetId)}", value);
            }
        }

        private List<ExporterProcessor> _processors;
        private TextAsset _serviceAccountJson;
        private string _googleSheetAppName;
        private string _sheetId;
        private VisualElement _diffContainer;

        [MenuItem("Tools/Translation Exporter")]
        private static void Open()
        {
            var window = GetWindow<TranslationExporterWindow>();
            window.titleContent = new("Translation Exporter");
            window.Show();
        }

        private void OnDestroy()
        {
            foreach (var processor in _processors)
            {
                processor.Sheet?.Dispose();
            }
        }

        private void CreateGUI()
        {
            _diffContainer = new();
            _diffContainer.style.flexGrow = 1;

            var root = rootVisualElement;
            root.Clear();
            _processors = GetProcessors();

            root.Add(ServiceAccountField());
            root.Add(GoogleAppNameField());
            root.Add(SheetIdField());
            root.Add(ToggleEnableProcessor());
            root.Add(FindDiffButton());

            root.Add(_diffContainer);
        }

        private async void FindDiff()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Find Diff", "", 0.9f);
                _diffContainer.Clear();

                
                var diffs = new List<DiffValue>();
                for (var i = 0; i < _processors.Count; i++)
                {
                    var processor = _processors[i];
                    if (!processor.Enable) continue;
                    processor.Sheet ??= GetSheet(processor.GroupId);
                    EditorUtility.DisplayProgressBar("Find Diff", $"{processor.GroupId}", (float)i / _processors.Count);
                    diffs.AddRange(await processor.FindDiffs());
                }

                _diffContainer.Add(new DiffView(diffs, exportAll: ExportAllDiff, importAll: ImportAll));
                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Error", e.Message, "Ok");
            }
        }

        private async void ExportAllDiff()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Export", "", 0.9f);

                for (var i = 0; i < _processors.Count; i++)
                {
                    var processor = _processors[i];
                    if (!processor.Enable) continue;
                    EditorUtility.DisplayProgressBar("Export", $"{processor.GroupId}", (float)i / _processors.Count);
                    await processor.ExportAllDiffs();
                }

                _diffContainer.Clear();
                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Error", e.Message, "Ok");
            }
        }

        private async void ImportAll()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Import", "", 0.9f);

                for (var i = 0; i < _processors.Count; i++)
                {
                    var processor = _processors[i];
                    if (!processor.Enable) continue;
                    EditorUtility.DisplayProgressBar("Import", $"{processor.GroupId}", (float)i / _processors.Count);
                    await processor.ImportAll();
                }

                _diffContainer.Clear();
                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Error", e.Message, "Ok");
            }
        }

        private SpreadSheetDictionary GetSheet(string sheetName)
        {
            var service = SpreadSheet.CreateService(ServiceAccountJson, GoogleSheetAppName);
            var sheet = new SpreadSheetDictionary(service, SheetId, sheetName);
            return sheet;
        }

        private List<ExporterProcessor> GetProcessors()
        {
            var result = new List<ExporterProcessor>();
            var typeCollection = TypeCache.GetTypesDerivedFrom<ExporterProcessor>();
            foreach (var type in typeCollection)
            {
                if (type.IsAbstract) continue;
                var processor = Activator.CreateInstance(type) as ExporterProcessor;
                result.Add(processor);
            }

            return result;
        }

        private VisualElement FindDiffButton()
        {
            var button = new Button();
            button.style.marginTop = 10;
            button.text = "Find Diff";
            button.clicked += FindDiff;
            return button;
        }

        private VisualElement ToggleEnableProcessor()
        {
            var container = new Foldout();
            container.text = "Options";
            container.value = EditorPrefs.GetBool($"{GetType().FullName}_foldOption", false);
            container.RegisterValueChangedCallback(evt =>
                EditorPrefs.SetBool($"{GetType().FullName}_foldOption", evt.newValue));

            container.style.marginTop = 10;

            var toggleContainer = new VisualElement();
            RedrawToggle();

            var buttonGroup = new VisualElement();
            buttonGroup.style.flexDirection = FlexDirection.Row;

            var enableAll = new Button();
            enableAll.text = "Enable All";
            enableAll.clicked += () => SetAllToggles(true);
            buttonGroup.Add(enableAll);

            var disableAll = new Button();
            disableAll.text = "Disable All";
            disableAll.clicked += () => SetAllToggles(false);
            buttonGroup.Add(disableAll);

            container.Add(toggleContainer);
            container.Add(buttonGroup);

            return container;

            void SetAllToggles(bool value)
            {
                foreach (var processor in _processors)
                {
                    processor.Enable = value;
                }

                RedrawToggle();
            }

            void RedrawToggle()
            {
                toggleContainer.Clear();
                toggleContainer.Add(ToggleGroup());
            }

            VisualElement ToggleGroup()
            {
                var group = new VisualElement();
                group.style.flexDirection = FlexDirection.Row;
                group.style.marginBottom = 2;
                var group1 = new VisualElement();
                var group2 = new VisualElement();
                group.Add(group1);
                group.Add(group2);
                for (var i = 0; i < _processors.Count; i++)
                {
                    var processor = _processors[i];
                    var toggle = new Toggle();
                    toggle.label = processor.GroupId;
                    toggle.value = processor.Enable;
                    toggle.RegisterValueChangedCallback(evt => { processor.Enable = evt.newValue; });
                    var targetGroup = i % 2 == 0 ? group1 : group2;
                    targetGroup.Add(toggle);
                }

                return group;
            }
        }

        private VisualElement SheetIdField()
        {
            var field = new TextField();
            field.label = nameof(SheetId);
            field.value = SheetId;
            field.RegisterValueChangedCallback(evt => { SheetId = evt.newValue; });
            return field;
        }

        private VisualElement GoogleAppNameField()
        {
            var field = new TextField();
            field.label = "App Name";
            field.tooltip = "Google Sheet App Name";
            field.value = GoogleSheetAppName;
            field.RegisterValueChangedCallback(evt => { GoogleSheetAppName = evt.newValue; });
            return field;
        }

        private VisualElement ServiceAccountField()
        {
            var field = new ObjectField();
            field.objectType = typeof(TextAsset);
            field.label = "Service Account";
            field.value = ServiceAccountJson;
            field.RegisterValueChangedCallback(evt => { ServiceAccountJson = (TextAsset)evt.newValue; });
            return field;
        }
    }
}