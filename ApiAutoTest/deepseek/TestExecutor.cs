using System;
using System.Linq;
using System.Threading.Tasks;
using TestAutomationEngine.Reporting;

namespace TestAutomationEngine.Core
{
    public class TestExecutor
    {
        public async Task<TestExecutionResult> ExecuteAsync(TestExecutionRequest request)
        {
            var result = new TestExecutionResult { StartTime = DateTime.UtcNow };
            request.IncludeHistory = request.IncludeHistory;  // <-- ensure this is set in the result for later use

            try
            {
                // 1. Deserialize project from JSON
                var project = request.Project;
               // var project = Project.FromJson(request.ProjectJson);
                if (project == null)
                {
                    result.Errors.Add("Invalid project JSON");
                    result.Success = false;
                    return result;
                }

                // 2. Determine which test plans to run
                var plansToRun = request.RunAllTestPlans
                    ? project.Children
                    : project.Children.Where(p => request.TestPlanNames.Contains(p.Name)).ToList();

                if (!plansToRun.Any())
                {
                    result.Errors.Add("No test plans matched the request.");
                    result.Success = false;
                    return result;
                }

                // 3. Run each selected test plan (optionally stop on first failure)
                bool hasFailure = false;
                foreach (var plan in plansToRun)
                {
                    if (request.StopOnFirstFailure && hasFailure)
                        break;

                    // Create a temporary project containing only this test plan
                    var singlePlanProject = new Project
                    {
                        Name = project.Name,
                        Environments = project.Environments,
                        GlobalVariables = project.GlobalVariables,
                        GlobalLogLevel = project.GlobalLogLevel
                    };
                    singlePlanProject.Children.Add(plan);

                    var runner = new ProjectRunner(singlePlanProject)
                    {
                        EnvironmentName = request.EnvironmentName,
                        LogLevel = request.LogLevel
                    };
                    var planResult = await runner.RunAsync();

                    var summary = new TestPlanResultSummary
                    {
                        Name = plan.Name,
                        Success = planResult.Success,
                        DurationMs = planResult.TestPlanResults.FirstOrDefault()?.DurationMs ?? 0,
                        AssertionsPassed = planResult.TestPlanResults.Sum(r => r.AssertionResults.Count(a => a.Passed)),
                        AssertionsFailed = planResult.TestPlanResults.Sum(r => r.AssertionResults.Count(a => !a.Passed)),
                        ErrorMessage = planResult.TestPlanResults.FirstOrDefault()?.Exception?.Message
                    };
                    result.TestPlanResults.Add(summary);

                    if (!planResult.Success)
                        hasFailure = true;
                }

                result.Success = !hasFailure;

                // 4. Optionally include full execution history and reports
                if (request.IncludeHistory)   // <-- reading from the request object
                {
                    // Run the entire project again to get complete data (or you could aggregate)
                    var fullRunner = new ProjectRunner(project) { EnvironmentName = request.EnvironmentName };
                    var fullResult = await fullRunner.RunAsync();
                    result.FullReportJUnit = new JUnitFormatter().Format(fullResult);
                    result.FullReportHtml = new HtmlFormatter().Format(fullResult);
                    result.RawExecutionData = fullResult;
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