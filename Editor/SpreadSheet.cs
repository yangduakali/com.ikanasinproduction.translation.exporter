using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json;
using UnityEngine;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource;
using Object = UnityEngine.Object;

namespace TranslationExporter
{
    /// <summary>
    /// Base wrapper class for Google Sheets API operations.
    /// Provides basic Read/Write methods for interacting with spreadsheet data.
    /// </summary>
    public class SpreadSheet : IDisposable
    {
        /// <summary>
        /// Creates a new SpreadSheet instance bound to a specific SheetsService and Spreadsheet ID.
        /// </summary>
        protected SpreadSheet(SheetsService service, string id)
        {
            Id = id;
            Service = service;
        }

        /// <summary>
        /// Creates a Google Sheets service from a service account JSON asset.
        /// </summary>
        /// <param name="serviceAccountJson">Unity TextAsset containing the service account credentials JSON.</param>
        /// <param name="appName">Application name (appears in Google API usage logs).</param>
        public static SheetsService CreateService(TextAsset serviceAccountJson, string appName)
        {
            var credential = GoogleCredential.FromJson(serviceAccountJson.text)
                .CreateScoped(SheetsService.Scope.Spreadsheets);

            return new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = appName
            });
        }

        /// <summary>
        /// Spreadsheet ID (Google Sheets document ID from its URL).
        /// </summary>
        protected readonly string Id;

        /// <summary>
        /// Google Sheets API service instance.
        /// </summary>
        protected readonly SheetsService Service;

        /// <summary>
        /// Reads a range from the sheet asynchronously.
        /// </summary>
        protected async Task<IList<IList<object>>> ReadAsync(string range)
        {
            var request = Service.Spreadsheets.Values.Get(Id, range);
            var response = await request.ExecuteAsync();
            return response.Values;
        }

        public async Task BatchWriteAsync(List<ValueRange> valueRanges)
        {
            if(valueRanges == null) return;
            if(valueRanges.Count == 0) return;
            var requestBody = new BatchUpdateValuesRequest
            {
                Data = valueRanges,
                ValueInputOption = "RAW"
            };
            var req = Service.Spreadsheets.Values.BatchUpdate(requestBody, Id);
            await req.ExecuteAsync();
        }

        /// <summary>
        /// Writes data to a specific range in the sheet asynchronously.
        /// </summary>
        protected async Task WriteAsync(string range, IList<IList<object>> values)
        {
            await CreateUpdateRequest(range, values).ExecuteAsync();
        }

        /// <summary>
        /// Creates an UpdateRequest object for the Google Sheets API.
        /// </summary>
        protected UpdateRequest CreateUpdateRequest(string range, IList<IList<object>> values)
        {
            var valueRange = new ValueRange { Values = values };
            var updateRequest = Service.Spreadsheets.Values.Update(valueRange, Id, range);
            updateRequest.ValueInputOption = UpdateRequest.ValueInputOptionEnum.RAW;
            return updateRequest;
        }

        public void Dispose()
        {
            Service?.Dispose();            
        }
    }

    /// <summary>
    /// Spreadsheet wrapper that treats the first row as column keys
    /// and the first column as row keys, allowing data to be accessed
    /// as a two-dimensional dictionary.
    /// </summary>
    public class SpreadSheetDictionary : SpreadSheet
    {
        /// <summary>
        /// Initializes a SpreadSheetDictionary with a specific sheet name.
        /// </summary>
        public SpreadSheetDictionary(SheetsService service, string id, string sheetName)
            : base(service, id)
        {
            SheetName = sheetName;
        }

        public readonly string SheetName;

        /// <summary>
        /// Stores sheet data as: dictionary[columnKey][rowKey] = value
        /// </summary>
        public readonly Dictionary<object, Dictionary<object, object>> dictionary = new();
        public readonly List<object> Columns = new();
        public readonly List<object> Rows = new();
        private readonly Dictionary<object, int> _columnIndexes = new();
        private readonly Dictionary<object, int> _rowIndexes = new();

        /// <summary>
        /// Reads the entire sheet into the in-memory dictionary.
        /// Column A and row 1 are treated as keys.
        /// </summary>
        public async Task UpdateAsync()
        {
            dictionary.Clear();
            _columnIndexes.Clear();
            _rowIndexes.Clear();
            Columns.Clear();
            Rows.Clear();

            var values = await ReadAsync(SheetName);
            var columns = values[0];

            // Map column headers to index
            for (int col = 1; col < columns.Count; col++)
            {
                var columnKey = columns[col];
                _columnIndexes[columnKey] = col;
                dictionary[columnKey] = new();
            }

            // Map row keys and assign values
            for (int rowIndex = 1; rowIndex < values.Count; rowIndex++)
            {
                var rowValues = values[rowIndex];
                var rowKey = rowValues[0];
                _rowIndexes[rowKey] = rowIndex;

                for (int col = 1; col < columns.Count; col++)
                {
                    var cellValue = (col < rowValues.Count) ? rowValues[col] : null;
                    dictionary[columns[col]][rowKey] = cellValue;
                }
            }

            Columns.AddRange(_columnIndexes.Keys);
            Rows.AddRange(_rowIndexes.Keys);
        }

        /// <summary>
        /// Creates a new row with the given row key if it does not already exist.
        /// </summary>
        public async Task CreateRow(object rowKey)
        {
            if (ContainsRow(rowKey)) return;

            _rowIndexes[rowKey] = _rowIndexes.Count + 1; // append at end
            foreach (var col in dictionary.Keys)
                dictionary[col][rowKey] = null;

            int rowIndex = _rowIndexes[rowKey];
            var range = $"{SheetName}!A{rowIndex + 1}";
            await WriteAsync(range, new List<IList<object>> { new List<object> { rowKey } });
        }

        public async Task DeleteRowsAsync(params object[] rowKeys)
        {
            if (rowKeys.Length == 0) return;
            var spreadsheet = await Service.Spreadsheets.Get(Id).ExecuteAsync();
            int? sheetId = 0;
            foreach (var sheet in spreadsheet.Sheets)
            {
                if (sheet.Properties.Title == SheetName)
                {
                    sheetId = sheet.Properties.SheetId;
                }
            }

            var deleteRequests = new List<DeleteDimensionRequest>();
            foreach (var rowKey in rowKeys)
            {
                var indexRow = GetRowIndex(rowKey) + 1;
                deleteRequests.Add(new()
                {
                    Range = new()
                    {
                        SheetId = sheetId,
                        Dimension = "ROWS",
                        StartIndex = indexRow - 1,
                        EndIndex = indexRow
                    }
                });
            }

            var orderedDeleteReq = deleteRequests.OrderByDescending(x => x.Range.StartIndex).ToList();
            var batchUpdateRequest = new BatchUpdateSpreadsheetRequest
            {
                Requests = orderedDeleteReq.ConvertAll(x => new Request() { DeleteDimension = x })
            };
            await Service.Spreadsheets.BatchUpdate(batchUpdateRequest, Id).ExecuteAsync();
        }

        /// <summary>
        /// Adds multiple rows to the Google Sheet from an array of objects.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the objects being added.  
        /// The property names in <typeparamref name="T"/> must match the sheet's column headers.
        /// </typeparam>
        /// <param name="objects">
        /// The array of objects to add as rows in the sheet.
        /// </param>
        /// <param name="columnKeyName">
        /// The property name that will be used as the key in column A (default: "Key").
        /// </param>
        /// <remarks>
        /// - Each object's properties are mapped to columns by matching property names to sheet headers.
        /// - The first property added will always be the `columnKeyName` value (column A).
        /// - Only properties that match existing column keys will be written.
        /// - The new rows will be appended after the existing rows in the sheet.
        /// - Will skip if the row key already exists
        /// </remarks>
        public async Task AddRowByObjectAsync<T>(T[] objects, string columnKeyName = "Key")
        {
            var range = GetRangeAddRowByObject(objects, columnKeyName, out var rangeValues);
            Debug.Log(range);
            await WriteAsync(range, rangeValues);
            await UpdateAsync();
        }

        public string GetRangeAddRowByObject<T>(T[] objects, string columnKeyName, out List<IList<object>> rangeValues)
        {
            // Determine where the new rows will start and end
            var startRowIndex = _rowIndexes.Count + 1; // index after the last existing row
            var endRowIndex = startRowIndex + objects.Length;

            // This will hold all rows to be written to the sheet in one batch
            rangeValues = new List<IList<object>>();

            for (int i = 0; i < objects.Length; i++)
            {
                var values = new List<object>();
                var obj = objects[i];

                // Convert object -> JSON -> Dictionary for dynamic property lookup
                var map = JsonConvert.DeserializeObject<Dictionary<object, object>>(JsonConvert.SerializeObject(obj));

                // Skip the object if it doesn't contain the key column
                if (!map.TryGetValue(columnKeyName, out var rowKey)) continue;

                // Add the key column value (first column in the sheet)
                if (ContainsRow(rowKey)) continue;
                values.Add(rowKey);

                // Add values for all columns, ensuring the order matches the sheet's header
                foreach (var (columnKey, _) in _columnIndexes)
                {
                    if (map.TryGetValue(columnKey, out var value))
                    {
                        values.Add(value);
                    }
                }

                // Add this row's values to the list of rows to be written
                rangeValues.Add(values);
            }
            return $"{SheetName}!A{startRowIndex + 1}:{GetColumnLetter(_columnIndexes.Count + 1)}{endRowIndex}";
        }

        /// <summary>
        /// Checks if a column exists by its key.
        /// </summary>
        public bool ContainsColumn(object columnKey) => _columnIndexes.ContainsKey(columnKey);

        /// <summary>
        /// Checks if a row exists by its key.
        /// </summary>
        public bool ContainsRow(object rowKey) => _rowIndexes.ContainsKey(rowKey);

        public int GetColumnIndex(object columnKey)
        {
            return _columnIndexes[columnKey];
        }

        public int GetRowIndex(object rowKey)
        {
            return _rowIndexes[rowKey];
        }

        /// <summary>
        /// Converts a 1-based column number to its Excel-style letter (A, B, ..., AA, AB, ...).
        /// </summary>
        public string GetColumnLetter(int colNumber)
        {
            string colLetter = "";
            while (colNumber > 0)
            {
                int mod = (colNumber - 1) % 26;
                colLetter = (char)('A' + mod) + colLetter;
                colNumber = (colNumber - mod) / 26;
            }
            return colLetter;
        }
    }

    /// <summary>
    /// Represents a diff for an entire row.
    /// </summary>
    public class DiffValue
    {
        public object RowKey = "";
        // Reference to asset , can be null if processor not provide
        // only use for display UI DiffView
        public Object Asset;
        public DiffColumn[] DiffColumns = Array.Empty<DiffColumn>();
    }

    /// <summary>
    /// Represents the difference in a single column between local and sheet values.
    /// </summary>
    public class DiffColumn
    {
        public object ColumnKey = "";
        public object LocalValue = "";
        public object SheetValue = "";
    }
}
