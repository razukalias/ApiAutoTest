// AssertionConverter.cs
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using TestAutomationEngine.Core;

public class AssertionConverter : JsonConverter<IAssertion>
{
    public override IAssertion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        var type = root.GetProperty("AssertionType").GetString();
        return type switch
        {
            "Direct" => JsonSerializer.Deserialize<DirectAssertion>(root.GetRawText(), options),
            "Variable" => JsonSerializer.Deserialize<VariableAssertion>(root.GetRawText(), options),
            _ => throw new NotSupportedException($"Unknown assertion type: {type}")
        };
    }

    public override void Write(Utf8JsonWriter writer, IAssertion value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("AssertionType", value switch
        {
            DirectAssertion => "Direct",
            VariableAssertion => "Variable",
            _ => throw new NotSupportedException()
        });

        // Serialize the object and copy all properties except the discriminator
        var json = JsonSerializer.Serialize(value, value.GetType(), options);
        using var doc = JsonDocument.Parse(json);
        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            if (prop.Name != "AssertionType")
                prop.WriteTo(writer);
        }
        writer.WriteEndObject();
    }
}