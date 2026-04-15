// AuthenticationStrategyConverter.cs
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using TestAutomationEngine.Core;

public class AuthenticationStrategyConverter : JsonConverter<IAuthenticationStrategy>
{
    public override IAuthenticationStrategy Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        var type = root.GetProperty("Type").GetString();
        return type switch
        {
            "OAuth2ClientCredentials" => JsonSerializer.Deserialize<OAuth2ClientCredentials>(root.GetRawText(), options),
            "ApiKey" => JsonSerializer.Deserialize<ApiKeyStrategy>(root.GetRawText(), options),
            "BearerToken" => JsonSerializer.Deserialize<BearerTokenStrategy>(root.GetRawText(), options),
            "WindowsIntegrated" => new WindowsIntegratedStrategy(),
            _ => throw new NotSupportedException($"Unknown auth type: {type}")
        };
    }

    public override void Write(Utf8JsonWriter writer, IAuthenticationStrategy value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("Type", value switch
        {
            OAuth2ClientCredentials => "OAuth2ClientCredentials",
            ApiKeyStrategy => "ApiKey",
            BearerTokenStrategy => "BearerToken",
            WindowsIntegratedStrategy => "WindowsIntegrated",
            _ => throw new NotSupportedException()
        });

        var json = JsonSerializer.Serialize(value, value.GetType(), options);
        using var doc = JsonDocument.Parse(json);
        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            if (prop.Name != "Type")
                prop.WriteTo(writer);
        }
        writer.WriteEndObject();
    }
}