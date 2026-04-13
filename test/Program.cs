using TestAutomationEngine.Core;
using TestAutomationEngine;
using Microsoft.Extensions.Logging;

class Program
{
    static async Task Main(string[] args)
    {
        // Load the JSON file (save the above JSON as "demo_project.json")
        string json = await File.ReadAllTextAsync("demo_project.json");
        var project = Project.FromJson(json);

        // Set up logging (optional)
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole().SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
        });

        // Run the project
        var runner = new ProjectRunner(project)
        {
            EnvironmentName = "Dev",   // matches environment in JSON
            LogLevel = TestAutomationEngine.Core.LogLevel.ComponentExecution
        };

        var result = await runner.RunAsync();

        // Output summary
        Console.WriteLine($"\n=== EXECUTION FINISHED ===");
        Console.WriteLine($"Success: {result.Success}");
        foreach (var planResult in result.TestPlanResults)
        {
            Console.WriteLine($"  {planResult.Component.Name}: {(planResult.Success ? "PASS" : "FAIL")} ({planResult.DurationMs}ms)");
            foreach (var assert in planResult.AssertionResults.Where(a => !a.Passed))
                Console.WriteLine($"    FAIL: {assert.Message}");
        }

        // Optionally save result as JSON or JUnit
        var junit = new TestAutomationEngine.Reporting.JUnitFormatter().Format(result);
        await File.WriteAllTextAsync("report.xml", junit);
    }
}