// DataSetStep.cs (extended)
using System.Data;
using System.IO;
using System.Text.Json;
using System.Xml;
using ClosedXML.Excel;
using CsvHelper;
using System.Globalization;

namespace TestAutomationEngine.Core
{
    public class DataSetStep : ComponentBase
    {
        public enum DataSourceType { Inline, Csv, Excel, Json, Xml }
        public DataSourceType Source { get; set; } = DataSourceType.Inline;
        public object? InlineData { get; set; }
        public string? FilePath { get; set; }
        public DataSetOptions Options { get; set; } = new();

        public class DataSetOptions
        {
            public bool HasHeader { get; set; } = true;
            public string? SheetName { get; set; }
        }

        public override string ComponentType => "DataSet";

        protected override async Task<ComponentResult> ExecuteCoreAsync(ExecutionContext context)
        {
            var table = new DataTable();
            var resolvedPath = !string.IsNullOrEmpty(FilePath) ? VariableResolver.Resolve(FilePath, context) : null;

            switch (Source)
            {
                case DataSourceType.Inline when InlineData != null:
                    // Assume InlineData is IEnumerable<object> or JSON array
                    LoadFromInline(table, InlineData);
                    break;
                case DataSourceType.Csv when resolvedPath != null:
                    using (var reader = new StreamReader(resolvedPath))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        using var dr = new CsvDataReader(csv);
                        table.Load(dr);
                    }
                    break;
                case DataSourceType.Excel when resolvedPath != null:
                    using (var workbook = new XLWorkbook(resolvedPath))
                    {
                        var worksheet = string.IsNullOrEmpty(Options.SheetName) ? workbook.Worksheet(1) : workbook.Worksheet(Options.SheetName);
                        var firstRow = worksheet.FirstRowUsed();
                        if (firstRow != null)
                        {
                            foreach (var cell in firstRow.CellsUsed())
                                table.Columns.Add(cell.GetString());
                            foreach (var row in worksheet.RowsUsed().Skip(1))
                            {
                                var newRow = table.NewRow();
                                int col = 0;
                                foreach (var cell in row.CellsUsed())
                                    newRow[col++] = cell.GetString();
                                table.Rows.Add(newRow);
                            }
                        }
                    }
                    break;
                case DataSourceType.Json when resolvedPath != null:
                    var json = await File.ReadAllTextAsync(resolvedPath);
                    LoadFromJson(table, json);
                    break;
                case DataSourceType.Xml when resolvedPath != null:
                    var xml = await File.ReadAllTextAsync(resolvedPath);
                    LoadFromXml(table, xml);
                    break;
            }

            return new ComponentResult { Component = this, Output = table };
        }

        private void LoadFromInline(DataTable table, object data)
        {
            // Simplistic: if data is IEnumerable<object>, convert each to row
            if (data is System.Collections.IEnumerable enumerable)
            {
                bool first = true;
                foreach (var item in enumerable)
                {
                    if (first)
                    {
                        if (item is System.Collections.IDictionary dict)
                        {
                            foreach (var key in dict.Keys)
                                table.Columns.Add(key.ToString());
                        }
                        first = false;
                    }
                    var row = table.NewRow();
                    if (item is System.Collections.IDictionary rowDict)
                    {
                        int i = 0;
                        foreach (var key in rowDict.Keys)
                            row[i++] = rowDict[key];
                    }
                    table.Rows.Add(row);
                }
            }
        }

        private void LoadFromJson(DataTable table, string json)
        {
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    if (table.Columns.Count == 0)
                    {
                        foreach (var prop in element.EnumerateObject())
                            table.Columns.Add(prop.Name);
                    }
                    var row = table.NewRow();
                    int i = 0;
                    foreach (var prop in element.EnumerateObject())
                        row[i++] = prop.Value.ToString();
                    table.Rows.Add(row);
                }
            }
        }

        private void LoadFromXml(DataTable table, string xml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var root = doc.DocumentElement;
            if (root != null)
            {
                var rows = root.SelectNodes("//*[local-name()='row']") ?? root.SelectNodes("*");
                bool first = true;
                foreach (XmlNode rowNode in rows)
                {
                    if (first)
                    {
                        foreach (XmlNode child in rowNode.ChildNodes)
                            table.Columns.Add(child.Name);
                        first = false;
                    }
                    var row = table.NewRow();
                    int i = 0;
                    foreach (XmlNode child in rowNode.ChildNodes)
                        row[i++] = child.InnerText;
                    table.Rows.Add(row);
                }
            }
        }
    }
}