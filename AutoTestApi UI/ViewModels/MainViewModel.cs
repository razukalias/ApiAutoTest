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
        [ObservableProperty] public Project? _project;
        [ObservableProperty] public ObservableCollection<Project> _projectRootCollection = new();
        [ObservableProperty] public ITestComponent? _selectedComponent;
      
        [ObservableProperty] public string _projectFilePath = "";
        [ObservableProperty] public string _environment = "Dev";
        [ObservableProperty] public bool _runAllTestPlans = true;
        [ObservableProperty] public string _statusMessage = "Ready";
        [ObservableProperty] public bool _isRunning;
        [ObservableProperty] public bool _isProjectLoaded;
        [ObservableProperty] public ObservableCollection<string> _outputMessages = new();
     
        public CancellationTokenSource? _cts;
        private string? _lastSavedSnapshot;

        partial void OnProjectChanged(Project? value)
        {
            IsProjectLoaded = value != null;
            ProjectRootCollection.Clear();
            if (value != null)
                ProjectRootCollection.Add(value);
            SelectedComponent = null;
        }


        [RelayCommand]
        public async Task LoadProject()
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
                    _lastSavedSnapshot = Project.ToJson();

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
        private void NewProject()
        {
            // Create a new empty project
            var newProject = new Project
            {
                Name = "New Project",
                Guid = Guid.NewGuid()
            };
            // Optionally add a default environment
            newProject.Environments["Dev"] = new Dictionary<string, object>();

            Project = newProject;
            OnProjectChanged(Project); // Will update ProjectRootCollection and IsProjectLoaded
            StatusMessage = "Created new project";
        }
        private bool HasUnsavedChanges()
        {
            if (Project == null) return false;
            var currentSnapshot = Project.ToJson();
            return currentSnapshot != _lastSavedSnapshot;
        }
        [RelayCommand]
        
        private void CloseProject()
        {
            if (Project == null) return;

            if (HasUnsavedChanges())
            {
                var result = MessageBox.Show(
                    "You have unsaved changes. Do you want to save before closing?",
                    "Unsaved Changes",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    SaveProjectCommand.Execute(null);
                    if (HasUnsavedChanges()) // User cancelled save
                        return;
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
                // If No, just close without saving
            }

            // Actually close
            Project = null;
            ProjectRootCollection.Clear();
            IsProjectLoaded = false;
            SelectedComponent = null;
            OutputMessages.Clear();
            _lastSavedSnapshot = null;
            StatusMessage = "Project closed";
        }

        [RelayCommand]
        public async Task SaveProject()
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
        public async Task RunProject()
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
        public void StopRun()
        {
            _cts?.Cancel();
        }
        [RelayCommand]      
        public void AddTestPlan()
        {
            if (Project == null) return;

            var newPlan = new TestPlan { Name = "New TestPlan" };
            Project.Children.Add(newPlan);

            RefreshTreeView();
            SelectedComponent = newPlan;
            StatusMessage = "Added new TestPlan";
        }

        [RelayCommand]
        public void AddComponent(string componentType)
        {
            if (SelectedComponent == null)
            {
                MessageBox.Show("Please select a container (TestPlan, Loop, etc.) first.", "No Container Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IContainerComponent? container = null;
            if (SelectedComponent is IContainerComponent selectedContainer)
                container = selectedContainer;
            else
                container = FindParentContainer(Project, SelectedComponent as ITestComponent);

            if (container == null)
            {
                MessageBox.Show("Cannot add component here.", "Invalid Container", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            if (container is ContainerComponentBase mutableContainer)
            {
                ITestComponent newComponent = CreateComponent(componentType);
                mutableContainer.Children.Add(newComponent);

                // Force TreeView refresh
                RefreshTreeView();

                SelectedComponent = newComponent;
                StatusMessage = $"Added {componentType}";
            }

        }
        public ITestComponent CreateComponent(string type) => type switch
        {
            "HttpStep" => new HttpStep { Name = "New HTTP Request" },
            "GraphQLStep" => new GraphQLStep { Name = "New GraphQL Query" },
            "Loop" => new Loop { Name = "New Loop" },
            "If" => new If { Name = "New If Condition", Condition = "true" },
            "While" => new While { Name = "New While Loop", Condition = "true" },
            "ScriptStep" => new ScriptStep { Name = "New Script" },
            _ => throw new ArgumentException($"Unknown component type: {type}")
        };
     

        [RelayCommand]
        public void DeleteSelectedComponent()
        {
            if (SelectedComponent == null) return;
            if (SelectedComponent is Project)
            {
                MessageBox.Show("Cannot delete the project root.", "Delete", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var component = SelectedComponent as ITestComponent;
            var parent = FindParentContainer(Project, component);

            if (parent is ContainerComponentBase mutableParent)
            {
                mutableParent.Children.Remove(component);
                RefreshTreeView();
                SelectedComponent = null;
                StatusMessage = $"Deleted '{component.Name}'";
            }
            else
            {
                MessageBox.Show("Parent container not found or not modifiable.", "Delete", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    
        public void RefreshTreeView()
        {
            // Recreate the root collection to trigger a full UI refresh
            var temp = ProjectRootCollection.ToList();
            ProjectRootCollection.Clear();
            foreach (var item in temp)
                ProjectRootCollection.Add(item);

            // Also raise property changed for Project
            OnPropertyChanged(nameof(Project));
        }
        public IContainerComponent? FindParentContainer(IContainerComponent root, ITestComponent? target)
        {
            if (target == null) return null;
            foreach (var child in root.Children)
            {
                if (child == target) return root;
                if (child is IContainerComponent childContainer)
                {
                    var found = FindParentContainer(childContainer, target);
                    if (found != null) return found;
                }
            }
            return null;
        }
    }
}