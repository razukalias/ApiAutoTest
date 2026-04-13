// TestComponentConverter.cs
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using TestAutomationEngine.Core;

public class TestComponentConverter : JsonConverter<ITestComponent>
{
    public override ITestComponent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        var type = root.GetProperty("ComponentType").GetString();
        return type switch
        {
            "Project" => JsonSerializer.Deserialize<Project>(root.GetRawText(), options),
            "TestPlan" => JsonSerializer.Deserialize<TestPlan>(root.GetRawText(), options),
            "Http" => JsonSerializer.Deserialize<HttpStep>(root.GetRawText(), options),
            "GraphQL" => JsonSerializer.Deserialize<GraphQLStep>(root.GetRawText(), options),
            "Sql" => JsonSerializer.Deserialize<SqlStep>(root.GetRawText(), options),
            "Script" => JsonSerializer.Deserialize<ScriptStep>(root.GetRawText(), options),
            "FileSystem" => JsonSerializer.Deserialize<FileSystemStep>(root.GetRawText(), options),
            "Convert" => JsonSerializer.Deserialize<ConvertStep>(root.GetRawText(), options),
            "DataSet" => JsonSerializer.Deserialize<DataSetStep>(root.GetRawText(), options),
            "Loop" => JsonSerializer.Deserialize<Loop>(root.GetRawText(), options),
            "If" => JsonSerializer.Deserialize<If>(root.GetRawText(), options),
            "While" => JsonSerializer.Deserialize<While>(root.GetRawText(), options),
            "Foreach" => JsonSerializer.Deserialize<Foreach>(root.GetRawText(), options),
            "Thread" => JsonSerializer.Deserialize<ThreadComponent>(root.GetRawText(), options),
            "Timer" => JsonSerializer.Deserialize<TimerStep>(root.GetRawText(), options),
            _ => throw new NotSupportedException($"Unknown component type: {type}")
        };
    }

    public override void Write(Utf8JsonWriter writer, ITestComponent value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, (object)value, options);
    }
}