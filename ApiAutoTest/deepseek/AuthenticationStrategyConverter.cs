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
        JsonSerializer.Serialize(writer, (object)value, options);
    }
}