using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TestAutomationEngine.Core;

namespace TestAutomation.UI.Wpf.ViewModels
{

    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty] private Project? _project;
        [ObservableProperty] private string _projectFilePath = "";
        [ObservableProperty] private string _environment = "Dev";
        [ObservableProperty] private bool _runAllTestPlans = true;
        [ObservableProperty] private string _statusMessage = "Ready";
        [ObservableProperty] private bool _isRunning;
        [ObservableProperty] private bool _isProjectLoaded;
        [ObservableProperty] private ObservableCollection<string> _outputMessages = new();
        [ObservableProperty] private ITestComponent? _selectedComponent;

        private CancellationTokenSource? _cts;

        partial void OnProjectChanged(Project? value)
        {
            IsProjectLoaded = value != null;
        }

        [RelayCommand]
        private async Task LoadProject()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                ProjectFilePath = dialog.FileName;
                try
                {
                    var json = await File.ReadAllTextAsync(ProjectFilePath);
                    Project = Project.FromJson(json);
                    StatusMessage = $"Loaded: {Project.Name}";
                    OutputMessages.Add($"Loaded project: {Project.Name}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load project: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private async Task SaveProject()
        {
            if (Project == null) return;
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json"
            };
            if (dialog.ShowDialog() == true)
            {
                await Project.SaveToFileAsync(dialog.FileName);
                StatusMessage = $"Saved: {dialog.FileName}";
            }
        }

        [RelayCommand]
        private async Task RunProject()
        {
            if (Project == null) return;
            _cts = new CancellationTokenSource();
            IsRunning = true;
            OutputMessages.Clear();
            StatusMessage = "Running...";

            try
            {
                var runner = new ProjectRunner(Project)
                {
                    EnvironmentName = Environment,
                    LogLevel = LogLevel.Verbose
                };
                // TODO: Add logging to OutputMessages
                var result = await Task.Run(() => runner.RunAsync(), _cts.Token);
                StatusMessage = result.Success ? "Passed" : "Failed";
                OutputMessages.Add($"=== Execution Finished ===");
                OutputMessages.Add($"Overall Success: {result.Success}");
                foreach (var planResult in result.TestPlanResults)
                {
                    OutputMessages.Add($"  {planResult.Component.Name}: {(planResult.Success ? "PASS" : "FAIL")} ({planResult.DurationMs}ms)");
                }
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Cancelled";
                OutputMessages.Add("*** Execution cancelled ***");
            }
            catch (Exception ex)
            {
                StatusMessage = "Error";
                OutputMessages.Add($"*** ERROR: {ex.Message} ***");
            }
            finally
            {
                IsRunning = false;
                _cts?.Dispose();
                _cts = null;
            }
        }

        [RelayCommand]
        private void StopRun()
        {
            _cts?.Cancel();
        }
    }
}