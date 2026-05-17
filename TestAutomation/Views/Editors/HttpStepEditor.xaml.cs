using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TestAutomation.UI.Wpf.ViewModels;
using TestAutomationEngine.Core;

namespace TestAutomation.UI.Wpf.Views.Editors;

public partial class HttpStepEditor : UserControl
{
    public static List<string> AvailableVariables { get; set; } = new();
    // ========== ASSERTIONS PICKER ==========

    private void AssertionsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // When user selects a different assertion row, reflect its SourceType
        // in the combo so the tree updates to match.
        if (AssertionsGrid.SelectedItem is AssertionViewModel vm)
        {
            string source = vm.SourceType;
            for (int i = 0; i < AssertionSourceCombo.Items.Count; i++)
            {
                if ((AssertionSourceCombo.Items[i] as ComboBoxItem)?.Content?.ToString() == source)
                {
                    AssertionSourceCombo.SelectedIndex = i;
                    return;
                }
            }
            AssertionSourceCombo.SelectedIndex = 0; // default to Response Body
        }
    }

    private void AssertionSourceCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CurrentStep == null) return;
        string? selected = (e.AddedItems.Count > 0
            ? (e.AddedItems[0] as ComboBoxItem)?.Content?.ToString()
            : null);

        switch (selected)
        {
            case "Response Body":
                ShowAssertionJsonPreview(CurrentStep.LastResponseBody);
                break;
            case "Request Body":
                ShowAssertionJsonPreview(CurrentStep.LastRequestContent);
                break;
            case "Response Headers":
                ShowAssertionHeadersPreview(CurrentStep.LastResponseHeaders);
                break;
            case "Status Code":
                AssertionJsonTreeView.Items.Clear();
                AssertionJsonTreeView.Items.Add(
                    new TreeViewItem
                    {
                        Header = CurrentStep.LastStatusCode ?? "N/A",
                        Tag = CurrentStep.LastStatusCode ?? ""
                    });
                break;
        }
    }

    private void ShowAssertionJsonPreview(string? json)
    {
        AssertionJsonTreeView.Items.Clear();
        if (string.IsNullOrWhiteSpace(json))
        {
            AssertionJsonTreeView.Items.Add(new TreeViewItem { Header = "No data available" });
            return;
        }
        try
        {
            var token = JToken.Parse(json);
            AssertionJsonTreeView.Items.Add(CreateJsonTreeItem(token, "$")); // reuse existing helper
        }
        catch
        {
            AssertionJsonTreeView.Items.Add(new TreeViewItem { Header = "Invalid JSON" });
        }
    }

    private void ShowAssertionHeadersPreview(string? headersText)
    {
        AssertionJsonTreeView.Items.Clear();
        if (string.IsNullOrWhiteSpace(headersText))
        {
            AssertionJsonTreeView.Items.Add(new TreeViewItem { Header = "No headers" });
            return;
        }
        foreach (var line in headersText.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split(": ", 2);
            var key = parts[0];
            var val = parts.Length > 1 ? parts[1] : "";
            AssertionJsonTreeView.Items.Add(new TreeViewItem
            {
                Header = $"{key}: {val}",
                Tag = key   // header name becomes the "path"
            });
        }
    }

    private void AssertionJsonTreeView_SelectedItemChanged(
        object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (AssertionJsonTreeView.SelectedItem is TreeViewItem item && item.Tag is string path)
        {
            ApplyPathToAssertion(path);
        }
    }

    private void ApplyPathToAssertion(string path)
    {
        string sourceType = (AssertionSourceCombo.SelectedItem as ComboBoxItem)
                            ?.Content?.ToString() ?? "Response Body";

        if (AssertionsGrid.SelectedItem is AssertionViewModel vm)
        {
            vm.Path = path;
            vm.SourceType = sourceType;
        }
        else
        {
            var newVm = new AssertionViewModel { Path = path, SourceType = sourceType };
           // _assertionViewModels?.Add(newVm);
            AssertionsGrid.SelectedItem = newVm;
        }
    }
    public HttpStepEditor()
    {
        InitializeComponent();
   
    }

    private HttpStep? CurrentStep => DataContext as HttpStep;
    private HttpStep? _currentStep;
    private ObservableCollection<HeaderItem>? _headerItems;
    //private ObservableCollection<AssertionViewModel>? _assertionViewModels;

    private void HttpStepEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is HttpStep)
        {
            LoadHeaders();
           LoadAssertions();  // was missing
        }
    }

    private void LoadAssertions()
    {
        if (DataContext is not ComponentBase comp) return;
        // The Assertions ObservableCollection is already bound via {Binding Assertions}
        // Just verify it's populated
        System.Diagnostics.Debug.WriteLine($"Assertions loaded: {comp.Assertions.Count}");
    }
    private void AddAssertion_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ComponentBase comp)
        {
            var newAssertion = new DirectAssertion
            {
                Behavior = AssertionBehavior.Assert,
                Path = "",
                Operator = Operator.Equals,
                ExpectedValue = ""
            };
            comp.Assertions.Add(newAssertion);

            // Select the new row
            AssertionsGrid.SelectedItem = newAssertion;
            AssertionsGrid.ScrollIntoView(newAssertion);
        }
    }
    private void LoadHeaders()
    {
        if (DataContext is HttpStep step)
        {
            _currentStep = step;
            _headerItems = new ObservableCollection<HeaderItem>(
                step.Headers.Select(kvp => new HeaderItem { Key = kvp.Key, Value = kvp.Value })
            );
            _headerItems.CollectionChanged += OnHeaderCollectionChanged;
            foreach (var item in _headerItems)
                item.PropertyChanged += OnHeaderItemChanged;
            HeadersGrid.ItemsSource = _headerItems;
        }
    }
    private void AddHeader_Click(object sender, RoutedEventArgs e)
    {
        _headerItems?.Add(new HeaderItem());
    }
    

    private void OnHeaderCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (_currentStep == null) return;
        // Rebuild dictionary
        _currentStep.Headers = _headerItems.ToDictionary(h => h.Key, h => h.Value);
    }

    private void OnHeaderItemChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Update dictionary when key or value changes
        _currentStep.Headers = _headerItems.ToDictionary(h => h.Key, h => h.Value);
    }
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