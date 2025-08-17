using System;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;

namespace TranslationExporter
{
    public class DiffView : VisualElement
    {
        private const int MaxDisplayCount = 20;
        public DiffView(List<DiffValue> diffs, Action exportAll, Action importAll)
        {
            style.paddingTop = 10;

            // Top header with action buttons
            var headerMenu = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.SpaceBetween,
                    minHeight = 30,
                }

            };

            var buttonExportAll = new Button
            {
                text = $"Export All ({diffs.Count}) >>",
                tooltip = "Export all local translations to the sheet. Will override existing sheet values."
            };
            buttonExportAll.clicked += exportAll;
            headerMenu.Add(buttonExportAll);

            var buttonImportAll = new Button
            {
                text = "<< Import All",
                tooltip = "Import all sheet values into local assets, overriding local values."
            };
            buttonImportAll.clicked += importAll;
            headerMenu.Add(buttonImportAll);

            Add(headerMenu);

            var scrollContainer = new ScrollView();
            var count = 0;
            foreach (var diff in diffs)
            {
                if (count >= MaxDisplayCount) break;
                count++;
                scrollContainer.Add(new DiffRowView(diff));
            }

            Add(scrollContainer);

            if (diffs.Count > MaxDisplayCount)
            {
                Add(new Label($"and {diffs.Count - MaxDisplayCount} more"));
            }
        }
    }

    /// <summary>
    /// Represents a single row's diff, including its key and per-column differences.
    /// </summary>
    public class DiffRowView : VisualElement
    {
        public DiffRowView(DiffValue diff)
        {
            style.paddingBottom = 5;
            style.paddingTop = 5;
            style.paddingLeft = 5;
            style.paddingRight = 5;

            Add(new RowHeader(diff));

            foreach (var diffColumn in diff.DiffColumns)
            {
                Add(new DiffColumnView(diffColumn));
            }
        }

        /// <summary>
        /// Displays the row's key as a header bar.
        /// </summary>
        private class RowHeader : VisualElement
        {
            public RowHeader(DiffValue diff)
            {
                
                style.backgroundColor = new Color(0.1568628f, 0.1568628f, 0.1568628f, 1);
                style.borderBottomLeftRadius = 1;
                style.borderBottomRightRadius = 1;
                style.borderTopLeftRadius = 1;
                style.borderTopRightRadius = 1;
                style.paddingBottom = 5;
                style.paddingTop = 5;
                style.paddingLeft = 5;
                style.paddingRight = 5;
                style.flexDirection = FlexDirection.Row;
                style.justifyContent = Justify.SpaceBetween;
                var label = new Label($"<b>{diff.RowKey}</b>");
                label.selection.isSelectable = true;
                Add(label);
                if (diff.Asset == null) return;
                var objectField = new ObjectField();
                objectField.objectType = diff.Asset.GetType();
                objectField.value = diff.Asset;
                Add(objectField);
            }
        }

        /// <summary>
        /// Displays the differences for a single column (local vs sheet value).
        /// </summary>
        private class DiffColumnView : VisualElement
        {
            public DiffColumnView(DiffColumn diff)
            {
                style.paddingBottom = 5;
                style.paddingTop = 5;
                style.paddingLeft = 5;
                style.paddingRight = 5;
                style.backgroundColor = new Color(0.3176471f, 0.3176471f, 0.3176471f, .5f);
                var headerLabel = new Label(diff.ColumnKey.ToString())
                {
                    style =
                    {
                        paddingBottom = 5,
                        paddingTop = 5,
                        paddingLeft = 5,
                        paddingRight = 5,
                        flexGrow = 0,
                        flexShrink = 0,
                        backgroundColor = new Color(0.17f, 0.17f, 0.17f, 1)
                    }
                };

                var horizontalContainer = new VisualElement();
                horizontalContainer.style.flexDirection = FlexDirection.Row;

                var valueContainer = new VisualElement
                {
                    style =
                    {
                        flexGrow = 1,
                        flexDirection = FlexDirection.Row,
                        borderBottomColor = new Color(1, 1, 1, 0.1f),
                        borderTopColor = new Color(1, 1, 1, 0.1f),
                        borderLeftColor = new Color(1, 1, 1, 0.1f),
                        borderRightColor = new Color(1, 1, 1, 0.1f),
                        borderBottomWidth = 1,
                        borderTopWidth = 1,
                        borderLeftWidth = 1,
                        borderRightWidth = 1
                    }
                };

                var localLabel = new WrapLabel($"{diff.LocalValue}", isLocal: true);
                var sheetLabel = new WrapLabel($"{diff.SheetValue}", isLocal: false);
                if (diff.LocalValue == null || diff.LocalValue.ToString() == "")
                {
                    style.backgroundColor = new Color(1, 0.4056603f, 0.4056603f, 0.1f);
                    sheetLabel.style.backgroundColor = new Color();
                }

                valueContainer.Add(localLabel);
                valueContainer.Add(sheetLabel);
                var header = new VisualElement();
                header.Add(headerLabel);
                horizontalContainer.Add(header);
                horizontalContainer.Add(valueContainer);

                Add(horizontalContainer);
            }
        }
    }

    public class WrapLabel : VisualElement
    {
        public WrapLabel(string text = "", bool isLocal = true)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            style.flexWrap = Wrap.Wrap;
            style.flexGrow = 1;

            var label = new Label(text)
            {
                style = { whiteSpace = WhiteSpace.Normal }
            };
            Add(label);

            style.paddingBottom = 5;
            style.paddingTop = 5;
            style.paddingLeft = 5;
            style.paddingRight = 5;

            var localColor = new Color(0, 0.6136636f, 0.9937106f, 0.1f);
            var sheetColor = new Color(0.05039846f, 0.9921569f, 0, 0.1f);
            style.backgroundColor = isLocal ? localColor : sheetColor;
        }
    }
}
