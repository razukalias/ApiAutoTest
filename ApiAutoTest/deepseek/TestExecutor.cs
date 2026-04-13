using System;
using System.Linq;
using System.Threading.Tasks;
using TestAutomationEngine.Reporting;

namespace TestAutomationEngine.Core
{
    public class TestExecutor
    {
        // TestExecutor.cs (modified section)
        public async Task<TestExecutionResult> ExecuteAsync(TestExecutionRequest request)
        {
            var result = new TestExecutionResult { StartTime = DateTime.UtcNow };
            result.IncludeHistory = request.IncludeHistory;

            try
            {
                var project = request.Project;
                if (project == null)
                {
                    result.Errors.Add("Invalid project JSON");
                    result.Success = false;
                    return result;
                }

                // Determine which test plans to run
                var plansToRun = request.RunAllTestPlans
                    ? project.Children
                    : project.Children.Where(p => request.TestPlanNames.Contains(p.Name)).ToList();

                if (!plansToRun.Any())
                {
                    result.Errors.Add("No test plans matched the request.");
                    result.Success = false;
                    return result;
                }

                // Create a filtered project containing only the selected test plans
                var filteredProject = new Project
                {
                    Name = project.Name,
                    Environments = project.Environments,
                    GlobalVariables = project.GlobalVariables,
                    GlobalLogLevel = project.GlobalLogLevel,
                    DataRetentionDays = project.DataRetentionDays
                };
                filteredProject.Children.AddRange(plansToRun);

                // Use ProjectRunner with component filters
                var runner = new ProjectRunner(filteredProject)
                {
                    EnvironmentName = request.EnvironmentName,
                    LogLevel = request.LogLevel,
                    ComponentFilters = request.ComponentFilters  // <-- Pass filters here
                };

                var fullResult = await runner.RunAsync();

                // Build summaries
                foreach (var planResult in fullResult.TestPlanResults)
                {
                    var summary = new TestPlanResultSummary
                    {
                        Name = planResult.Component.Name,
                        Success = planResult.Success,
                        DurationMs = planResult.DurationMs,
                        AssertionsPassed = planResult.AssertionResults.Count(a => a.Passed),
                        AssertionsFailed = planResult.AssertionResults.Count(a => !a.Passed),
                        ErrorMessage = planResult.Exception?.Message
                    };
                    result.TestPlanResults.Add(summary);
                }

                result.Success = fullResult.Success;
                result.RawExecutionData = fullResult;

                if (request.IncludeHistory)
                {
                    result.FullReportJUnit = new Reporting.JUnitFormatter().Format(fullResult);
                    result.FullReportHtml = new Reporting.HtmlFormatter().Format(fullResult);
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
                result.Success = false;
            }

            result.EndTime = DateTime.UtcNow;
            return result;
        }
    }
}