using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TestAutomationEngine.Core
{
    internal class HttpMethodConverter : JsonConverter<HttpMethod>
    {
        public override HttpMethod Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string method = reader.GetString();
            return method?.ToUpperInvariant() switch
            {
                "GET" => HttpMethod.Get,
                "POST" => HttpMethod.Post,
                "PUT" => HttpMethod.Put,
                "DELETE" => HttpMethod.Delete,
                "PATCH" => HttpMethod.Patch,
                "HEAD" => HttpMethod.Head,
                "OPTIONS" => HttpMethod.Options,
                "TRACE" => HttpMethod.Trace,
                _ => new HttpMethod(method)
            };
        }
        public override void Write(Utf8JsonWriter writer, HttpMethod value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.Method);
    }
}