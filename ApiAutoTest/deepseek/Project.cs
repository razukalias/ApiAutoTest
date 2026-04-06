// Project.cs
using System.Collections.Generic;
using System.Text.Json;

namespace TestAutomationEngine.Core
{
    public class Project : ContainerComponentBase
    {
        public Dictionary<string, Dictionary<string, object>> Environments { get; set; } = new();
        public Dictionary<string, Variable> GlobalVariables { get; set; } = new();
        public LogLevel GlobalLogLevel { get; set; } = LogLevel.Summary;
        public int DataRetentionDays { get; set; } = 0;

        public override string ComponentType => "Project";
        /*
        public string ToJson()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            options.Converters.Add(new TestComponentConverter());
            return JsonSerializer.Serialize(this, options);
        }*/
        public string ToJson() => ProjectSerializer.Serialize(this);
        /*
        public static Project FromJson(string json)
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new TestComponentConverter());
            return JsonSerializer.Deserialize<Project>(json, options);
        }
        */
        public static Project FromJson(string json) => ProjectSerializer.Deserialize(json);
        public async Task SaveToFileAsync(string path) => await File.WriteAllTextAsync(path, ToJson());
        public static async Task<Project> LoadFromFileAsync(string path) => FromJson(await File.ReadAllTextAsync(path));
        protected override async Task<ComponentResult> ExecuteCoreAsync(ExecutionContext context)
        {
            // Load environment constants as immutable variables
            if (context.EnvironmentName != null && Environments.TryGetValue(context.EnvironmentName, out var envConstants))
            {
                foreach (var (key, value) in envConstants)
                    context.SetVariable(key, value, isMutable: false);
            }
            foreach (var (key, var) in GlobalVariables)
                context.SetVariable(key, var.Value, var.IsMutable, var.IsSensitive);

            return await ExecuteChildrenAsync(context);
        }
      

    }

    public enum LogLevel { Off, Errors, Summary, ComponentExecution, Verbose }
}