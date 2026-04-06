using System.Text.Json;
using System.Text.Json.Serialization;
using TestAutomationEngine.Core;
using TestAutomationEngine.Reporting;

namespace JsonProjectRunner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string commandJson = File.ReadAllText("runCommand.json");
            //var request = JsonSerializer.Deserialize<TestExecutionRequest>(commandJson);
            var request = JsonSerializer.Deserialize<TestExecutionRequest>(commandJson, ProjectSerializer.Options);
            var executor = new TestExecutor();
            var result = await executor.ExecuteAsync(request);

            Console.WriteLine($"Success: {result.Success}");
            Console.WriteLine($"Duration: {result.Duration}");
            foreach (var planResult in result.TestPlanResults)
            {
                Console.WriteLine($"  {planResult.Name}: {(planResult.Success ? "PASS" : "FAIL")} ({planResult.DurationMs}ms)");
            }
            if (result.IncludeHistory)
            {
                File.WriteAllText("report.html", result.FullReportHtml);
                Console.WriteLine("Full HTML report saved to report.html");
            }
        }
    }
}