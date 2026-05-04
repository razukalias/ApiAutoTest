using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TestAutomationEngine.Core;

namespace TestAutomation.UI.Wpf.Views.Editors;

public partial class HttpStepEditor : UserControl
{
    public static List<string> AvailableVariables { get; set; } = new();
    public HttpStepEditor()
    {
        InitializeComponent();
    }

    private HttpStep? CurrentStep => DataContext as HttpStep;
    private void SetTargetVariableFromPath(ExtractionRule rule, string path)
    {
        var parts = path.Split(new[] { '.', '[' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 0)
        {
            string last = parts.Last().Replace("]", "").Replace("\"", "");
            if (!string.IsNullOrWhiteSpace(last))
                rule.TargetVariable = last;
        }
    }
    private void ApplySourcePathToExtraction(string sourcePath)
    {
        // Get the current source type from the dropdown
        string? currentSourceType = (SourceTypeCombo.SelectedItem as ComboBoxItem)?.Content?.ToString();

        if (ExtractionDataGrid.SelectedItem is ExtractionRule rule)
        {
            rule.Source = sourcePath;
            rule.SourceType = currentSourceType ?? string.Empty;   // 👈 set it
            SetTargetVariableFromPath(rule, sourcePath);
        }
        else
        {
            var newRule = new ExtractionRule();
            newRule.Source = sourcePath;
            newRule.SourceType = currentSourceType ?? string.Empty; // 👈 set it
            SetTargetVariableFromPath(newRule, sourcePath);

            var extractions = DataContext is HttpStep step ? step.Extractions : null;
            if (extractions != null)
            {
                extractions.Add(newRule);
                ExtractionDataGrid.Items.Refresh();
                ExtractionDataGrid.SelectedItem = newRule;
                ExtractionDataGrid.ScrollIntoView(newRule);
            }
        }
    }
    private void ApplySourcePathToExtractionR(string sourcePath)
    {
        if (ExtractionDataGrid.SelectedItem is ExtractionRule rule)
        {
            // Update the selected rule
            rule.Source = sourcePath;
            SetTargetVariableFromPath(rule, sourcePath);
        }
        else
        {
            // No selection → create a new rule
            var newRule = new ExtractionRule();
            newRule.Source = sourcePath;
            SetTargetVariableFromPath(newRule, sourcePath);

            // Add to the collection
            var extractions = DataContext is HttpStep step ? step.Extractions : null;
            if (extractions != null)
            {
                extractions.Add(newRule);
                // Refresh the DataGrid to show the new row, then select it
                ExtractionDataGrid.Items.Refresh();
                ExtractionDataGrid.SelectedItem = newRule;
                // Optionally scroll into view
                ExtractionDataGrid.ScrollIntoView(newRule);
            }
        }
    }
    private void JsonTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (JsonTreeView.SelectedItem is TreeViewItem selectedItem && selectedItem.Tag is string path)
        {
            ApplySourcePathToExtraction(path);
        }
    }

    private void PreviewJsonTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (sender is TreeView tv && tv.SelectedItem is TreeViewItem item && item.Tag is string path)
            UpdateSelectedExtractionRule(path);

       
    }

    private void HeadersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid dg && dg.SelectedItem is KeyValuePair<string, string> header)
            UpdateSelectedExtractionRule(header.Key);
    }

    // ========== SOURCE DROPDOWN ==========
    private void SourceTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CurrentStep == null) return;

        string? selected = (e.AddedItems[0] as ComboBoxItem)?.Content?.ToString();
        switch (selected)
        {
            case "Response Body":
                ShowJsonPreview(CurrentStep.LastResponseBody);
                break;
            case "Request Body":
                ShowJsonPreview(CurrentStep.LastRequestContent);
                break;
            case "Response Headers":
                ShowHeadersPreview(CurrentStep.LastResponseHeaders);
                break;
            case "Status Code":
                JsonTreeView.Items.Clear();
                JsonTreeView.Items.Add(new TreeViewItem { Header = CurrentStep.LastStatusCode ?? "N/A" });
                break;
        }
    }

    private void ShowJsonPreview(string? json)
    {
        JsonTreeView.Items.Clear();
        if (string.IsNullOrWhiteSpace(json))
        {
            JsonTreeView.Items.Add(new TreeViewItem { Header = "No data available" });
            return;
        }

        try
        {
            JToken token = JToken.Parse(json);
            JsonTreeView.Items.Add(CreateJsonTreeItem(token, "$"));
        }
        catch
        {
            JsonTreeView.Items.Add(new TreeViewItem { Header = "Invalid JSON" });
        }
    }

    private void ShowHeadersPreview(string? headersText)
    {
        JsonTreeView.Items.Clear();
        if (string.IsNullOrWhiteSpace(headersText))
        {
            JsonTreeView.Items.Add(new TreeViewItem { Header = "No headers" });
            return;
        }

        var headers = headersText.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(line =>
                                 {
                                     var parts = line.Split(": ", 2);
                                     return new KeyValuePair<string, string>(parts[0], parts.Length > 1 ? parts[1] : "");
                                 })
                                 .ToList();

        // For headers we reuse the TreeView but show simple items
        foreach (var header in headers)
        {
            var item = new TreeViewItem
            {
                Header = $"{header.Key}: {header.Value}",
                Tag = header.Key   // clicking will use the header name as source
            };
            JsonTreeView.Items.Add(item);
        }
    }

    // ========== JSON TREE ==========
    private TreeViewItem CreateJsonTreeItem(JToken token, string path)
    {
        string header = token switch
        {
            JObject obja => $"{{...}} ({obja.Count} properties)",
            JArray arr => $"[...] ({arr.Count} items)",
            JValue val => val.ToString(),
            _ => token.ToString()
        };

        var item = new TreeViewItem { Header = header, Tag = path };

        if (token is JObject obj)
        {
            foreach (var prop in obj.Properties())
                item.Items.Add(CreateJsonTreeItem(prop.Value, $"{path}.{prop.Name}"));
        }
        else if (token is JArray arr)
        {
            for (int i = 0; i < arr.Count; i++)
                item.Items.Add(CreateJsonTreeItem(arr[i], $"{path}[{i}]"));
        }
        return item;
    }

  

    // ========== VARIABLE DROPDOWN ==========
    private void VariableCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (VariableCombo.SelectedItem is string variableName)
        {
            if (ExtractionDataGrid.SelectedItem is ExtractionRule rule)
            {
                rule.TargetVariable = variableName;
            }
        }
    }

    // ========== HELPER ==========
    private void UpdateSelectedExtractionRule(string sourcePath)
    {
        if (ExtractionDataGrid.SelectedItem is ExtractionRule rule)
        {
            rule.Source = sourcePath;

            // Auto‑suggest a target variable name from the last segment
            var parts = sourcePath.Split(new[] { '.', '[' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
                rule.TargetVariable = parts.Last().Replace("]", "");
        }
        else
        {
            MessageBox.Show("Please select a row in the extraction grid first.", "No Row Selected", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}