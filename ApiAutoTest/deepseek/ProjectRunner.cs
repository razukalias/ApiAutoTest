// ProjectRunner.cs (modified)
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace TestAutomationEngine.Core
{
    public class ProjectRunner
    {
        private readonly Project _project;
        public string EnvironmentName { get; set; } = string.Empty;
        public LogLevel LogLevel { get; set; } = LogLevel.Summary;
        public IDebugger? Debugger { get; set; }

        /// <summary>Component filters to apply before execution.</summary>
        public List<ComponentFilter>? ComponentFilters { get; set; }

        public ProjectRunner(Project project)
        {
            _project = project;
        }

        public async Task<ProjectResult> RunAsync()
        {
            // Apply component filters to the project tree if provided
            if (ComponentFilters != null && ComponentFilters.Count > 0)
            {
                ComponentFilterer.ApplyFilters(_project, ComponentFilters);
            }

            var context = new ExecutionContext(minLogLevel: LogLevel, debugger: Debugger)
            {
                EnvironmentName = EnvironmentName
            };
            var result = new ProjectResult
            {
                ProjectName = _project.Name,
                EnvironmentUsed = EnvironmentName,
                LogLevel = LogLevel
            };

            foreach (var plan in _project.Children)
            {
                try
                {
                    var planResult = await plan.ExecuteAsync(context);
                    result.TestPlanResults.Add(planResult);
                }
                catch (AssertionFailedException ex)
                {
                    var failedResult = new ComponentResult { Component = plan, HasUnhandledException = true, Exception = ex };
                    result.TestPlanResults.Add(failedResult);
                    context.Log(LogLevel.Errors, $"Fatal assertion failed in TestPlan '{plan.Name}': {ex.Message}");
                }
            }
            return result;
        }

        public async Task<ProjectResult> RunAndSaveAsync(string outputDirectory)
        {
            var result = await RunAsync();
            var fileName = $"{_project.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var filePath = Path.Combine(outputDirectory, fileName);
            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);

            if (_project.DataRetentionDays > 0)
            {
                var cutoff = DateTime.Now.AddDays(-_project.DataRetentionDays);
                foreach (var oldFile in Directory.GetFiles(outputDirectory, $"{_project.Name}_*.json"))
                {
                    if (File.GetCreationTime(oldFile) < cutoff)
                        File.Delete(oldFile);
                }
            }
            return result;
        }

        public static async Task<ProjectResult> RunFromJsonAsync(string json, string environmentName, LogLevel logLevel = LogLevel.Summary)
        {
            var project = Project.FromJson(json);
            var runner = new ProjectRunner(project) { EnvironmentName = environmentName, LogLevel = logLevel };
            return await runner.RunAsync();
        }
    }
}