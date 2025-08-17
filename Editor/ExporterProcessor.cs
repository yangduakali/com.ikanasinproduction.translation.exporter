using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace TranslationExporter
{
    /// <summary>
    /// Base class for processors that synchronize localized data between a local source (e.g. Unity assets)
    /// and a Google Spreadsheet.  
    /// 
    /// This class provides diff detection, synchronization, and helper methods to add, update, 
    /// and delete rows in the target sheet.
    /// </summary>
    public abstract class ExporterProcessor
    {
        /// <summary>
        /// Whether this exporter is enabled. Stored in Unity <see cref="EditorPrefs"/> 
        /// using the class type name as the key.
        /// </summary>
        public bool Enable
        {
            get => EditorPrefs.GetBool($"{GetType().FullName}_{nameof(Enable)}", true);
            set => EditorPrefs.SetBool($"{GetType().FullName}_{nameof(Enable)}", value);
        }

        /// <summary>
        /// Reference to the spreadsheet dictionary wrapper (Google Sheets abstraction).
        /// Provides access to rows, columns, and cell dictionary lookups.
        /// </summary>
        public SpreadSheetDictionary Sheet { get; internal set; }

        /// <summary>
        /// Unique group identifier for this exporter.  
        /// Used to target the correct sheet/tab inside the spreadsheet.
        /// </summary>
        public abstract string GroupId { get; }

        // --- Diff collections ---
        // Stored separately for batch cell updates.

        /// <summary>
        /// Diff values for existing rows where some cells have changed.  
        /// </summary>
        private readonly List<DiffValue> _diffs = new();

        /// <summary>
        /// Diff values representing entirely new rows (not present in the sheet).  
        /// These will be appended to the sheet.
        /// </summary>
        private readonly List<DiffValue> _newValueDiff = new();

        /// <summary>
        /// Diff values representing rows that exist in the sheet but no longer exist locally.  
        /// These rows will be deleted.
        /// </summary>
        private readonly List<DiffValue> _deleteDiff = new();

        /// <summary>
        /// Compare local data with the current sheet state to compute differences.  
        /// Differences are categorized into updated cells, new rows, and deleted rows.
        /// </summary>
        /// <returns>A list of <see cref="DiffValue"/> objects describing all diffs.</returns>
        internal async Task<List<DiffValue>> FindDiffs()
        {
            var result = new List<DiffValue>();
            _diffs.Clear();
            _deleteDiff.Clear();
            _newValueDiff.Clear();

            await Sheet.UpdateAsync();
            var rows = new List<object>();
            rows.AddRange(Sheet.Rows);

            var localValues = GetLocalizeData();
            foreach (var localizeString in localValues)
            {
                var rowKey = localizeString.Key;
                var paramMaps = ConvertObjectToMap(localizeString);
                var diffColumns = new List<DiffColumn>();
                var findDiff = false;
                rows.Remove(rowKey);
                var rowIsExist = Sheet.ContainsRow(rowKey);

                // Compare column values
                foreach (var (key, value) in paramMaps)
                {
                    if (!Sheet.Columns.Contains(key)) continue;
                    object sheetValue = null;
                    if (rowIsExist)
                    {
                        sheetValue = Sheet.dictionary[key][rowKey];
                        if ($"{sheetValue}" == $"{value}") continue;
                    }

                    findDiff = true;
                    diffColumns.Add(new()
                    {
                        ColumnKey = key,
                        LocalValue = value,
                        SheetValue = sheetValue
                    });
                }

                if (!findDiff) continue;
                var diff = new DiffValue
                {
                    RowKey = localizeString.Key,
                    Asset = localizeString.GetAsset(),
                    DiffColumns = diffColumns.ToArray(),
                };

                result.Add(diff);

                if (rowIsExist)
                {
                    _diffs.Add(diff);
                }
                else
                {
                    _newValueDiff.Add(diff);
                }
            }

            // Remaining rows exist in sheet but not locally → mark for deletion
            foreach (var row in rows)
            {
                var diffColumns = new List<DiffColumn>();
                foreach (var col in Sheet.Columns)
                {
                    diffColumns.Add(new()
                    {
                        ColumnKey = col,
                        SheetValue = Sheet.dictionary[col][row]
                    });
                }

                var diff = new DiffValue()
                {
                    RowKey = row,
                    DiffColumns = diffColumns.ToArray()
                };
                _deleteDiff.Add(diff);
                result.Add(diff);
            }

            return result;
        }

        /// <summary>
        /// Export all differences from local to sheet:
        /// - Add new rows.
        /// - Update existing cells.
        /// - Delete rows missing in local data.
        /// </summary>
        internal async Task ExportAllDiffs()
        {
            var valueRange = new List<ValueRange>();

            // Handle new rows
            var newRows = new List<Dictionary<object, object>>();
            foreach (var val in _newValueDiff)
            {
                var map = new Dictionary<object, object> { { "Key", val.RowKey } };
                foreach (var diff in val.DiffColumns)
                {
                    map.Add(diff.ColumnKey, diff.LocalValue);
                }

                newRows.Add(map);
            }

            var newRowRange = Sheet.GetRangeAddRowByObject(newRows.ToArray(), "Key", out var newRowValue);
            valueRange.Add(new() { Range = newRowRange, Values = newRowValue });

            // Handle updated cells
            foreach (var val in _diffs)
            {
                var rowKey = val.RowKey;
                var rowIndex = Sheet.GetRowIndex(rowKey);
                foreach (var diff in val.DiffColumns)
                {
                    var columnKey = diff.ColumnKey;
                    var columnIndex = Sheet.GetColumnIndex(columnKey);
                    var range = $"{Sheet.SheetName}!{Sheet.GetColumnLetter(columnIndex + 1)}{rowIndex + 1}";
                    valueRange.Add(new()
                    {
                        Range = range, Values = new List<IList<object>> { new List<object> { diff.LocalValue } }
                    });
                }
            }

            await Sheet.BatchWriteAsync(valueRange);
            await DeleteDiff();
        }

        /// <summary>
        /// Import all sheet values into local data:
        /// - Apply updated values.
        /// - Delete rows missing in local data.
        /// </summary>
        internal async Task ImportAll()
        {
            GetLocalizeData();
            foreach (var var in _diffs)
            {
                foreach (var diff in var.DiffColumns)
                {
                    UpdateValue(var.RowKey, diff.ColumnKey, diff.SheetValue);
                }
            }

            await DeleteDiff();
        }

        /// <summary>
        /// Delete rows from the sheet that no longer exist in the local data.
        /// Executed both Export and Import
        /// </summary>
        private async Task DeleteDiff()
        {
            if (_deleteDiff.Count == 0) return;
            await Sheet.DeleteRowsAsync(_deleteDiff.ConvertAll(x => x.RowKey).ToArray());
        }

        /// <summary>
        /// Retrieve all localized data objects from the local source (to be compared with sheet values).
        /// Must be implemented by the concrete exporter.
        /// </summary>
        protected abstract List<ILocalizeString> GetLocalizeData();

        /// <summary>
        /// Update a single local value with the corresponding sheet value.  
        /// Override to apply sheet → local synchronization.
        /// </summary>
        protected virtual void UpdateValue(object rowKey, object columnKey, object value) { }

        /// <summary>
        /// Convert an object into a key-value dictionary using JSON serialization.  
        /// Useful for comparing arbitrary data models.
        /// </summary>
        protected Dictionary<object, object> ConvertObjectToMap(object obj)
        {
            return obj == null
                ? new()
                : JsonConvert.DeserializeObject<Dictionary<object, object>>(JsonConvert.SerializeObject(obj));
        }
    }

    /// <summary>
    /// Interface representing a localized string entry.  
    /// Each entry is uniquely identified by <see cref="Key"/>.
    /// </summary>
    public interface ILocalizeString
    {
        /// <summary>
        /// The unique key identifying this row/entry in the sheet.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// An optional reference to a Unity <see cref="Object"/> (e.g., Sprite, AudioClip, Prefab).  
        /// If provided, this asset can be displayed, previewed, or bound in the UI alongside the localized text.  
        /// </summary>
        Object GetAsset()
        {
            return null;
        }
    }
}