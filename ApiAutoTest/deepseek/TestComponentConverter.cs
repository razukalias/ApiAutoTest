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
            // Add all component types...
            _ => throw new NotSupportedException($"Unknown component type: {type}")
        };
    }

    public override void Write(Utf8JsonWriter writer, ITestComponent value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, (object)value, options);
    }
}