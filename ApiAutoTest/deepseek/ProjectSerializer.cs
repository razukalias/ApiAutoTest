// ProjectSerializer.cs (add to TestAutomationEngine.Core)
using System.Text.Json;
using System.Text.Json.Serialization;
using CsvHelper.TypeConversion;

namespace TestAutomationEngine.Core
{
    public static class ProjectSerializer
    {
        private static readonly JsonSerializerOptions _options;
        public static JsonSerializerOptions Options => _options;

        static ProjectSerializer()
        {
            _options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
            // Built‑in converters
            _options.Converters.Add(new JsonStringEnumConverter());
            _options.Converters.Add(new HttpMethodConverter());
            _options.Converters.Add(new TimeSpanConverter());
            _options.Converters.Add(new UriConverter());
            // Polymorphic converters for library types
            _options.Converters.Add(new TestComponentConverter());
            _options.Converters.Add(new AssertionConverter());
            _options.Converters.Add(new AuthenticationStrategyConverter());
        }

        public static string Serialize(Project project) => JsonSerializer.Serialize(project, _options);
        public static Project Deserialize(string json) => JsonSerializer.Deserialize<Project>(json, _options);
    }
}