using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TestAutomationEngine.Core;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TestAutomation.UI.Wpf.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private Project _project;
    [ObservableProperty] private object _selectedComponent; // Use object to allow selection of non-container items
    [ObservableProperty] private ObservableCollection<string> _outputMessages = new();
    [ObservableProperty] private string _statusMessage = "Ready";
    [ObservableProperty] private string _currentStep = "";
    [ObservableProperty] private ObservableCollection<KeyValuePair<string, Dictionary<string, object>>> _environments;
    [ObservableProperty] private KeyValuePair<string, Dictionary<string, object>> _selectedEnvironment;
    [ObservableProperty] private ObservableCollection<LogLevel> _logLevels = new(Enum.GetValues<LogLevel>());
    [ObservableProperty] private LogLevel _selectedLogLevel = LogLevel.Summary;
    [ObservableProperty] private ObservableCollection<string> _toolboxItems = new() { "HttpStep", "GraphQLStep", "Loop", "Script" };

    public MainViewModel()
    {
        Project = new Project { Name = "New Project" };
        Environments = new();
    }

    [RelayCommand]
    private void AddHttpStep()
    {
        // Determine the container to add to
        IContainerComponent? container = null;

        if (SelectedComponent is IContainerComponent selectedContainer)
        {
            container = selectedContainer;
        }
        else if (SelectedComponent is ITestComponent leaf)
        {
            // Find the parent container of the selected leaf
            container = FindParentContainer(Project, leaf);
        }
        else
        {
            // Nothing selected, add to the first TestPlan or Project
            container = Project.Children.FirstOrDefault() as IContainerComponent ?? Project;
        }

        if (container == null)
        {
            MessageBox.Show("No valid container selected.", "Cannot Add", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Cast to mutable container to add
        if (container is ContainerComponentBase mutableContainer)
        {
            var step = new HttpStep { Name = "New HTTP Step" };
            mutableContainer.Children.Add(step);

            // Force UI refresh by raising property changed on Project
            OnPropertyChanged(nameof(Project));
            StatusMessage = $"Added HttpStep to '{mutableContainer.Name}'";
        }
        else
        {
            MessageBox.Show("Cannot modify this container type.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void DeleteSelected()
    {
        if (SelectedComponent == null) return;

        // Cannot delete the Project itself
        if (SelectedComponent is Project)
        {
            MessageBox.Show("Cannot delete the project root.", "Delete", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var componentToDelete = SelectedComponent as ITestComponent;
        var parent = FindParentContainer(Project, componentToDelete);

        if (parent is ContainerComponentBase mutableParent)
        {
            mutableParent.Children.Remove(componentToDelete);
            OnPropertyChanged(nameof(Project));
            SelectedComponent = null;
            StatusMessage = $"Deleted '{componentToDelete.Name}'";
        }
        else
        {
            MessageBox.Show("Parent container not found or not modifiable.", "Delete", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Helper to find the parent container of a given component
    private IContainerComponent? FindParentContainer(IContainerComponent root, ITestComponent target)
    {
        foreach (var child in root.Children)
        {
            if (child == target)
                return root;
            if (child is IContainerComponent childContainer)
            {
                var found = FindParentContainer(childContainer, target);
                if (found != null)
                    return found;
            }
        }
        return null;
    }

    // ... other commands (Run, Open, Save, etc.)
}