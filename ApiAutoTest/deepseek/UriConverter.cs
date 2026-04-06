using System.Text.Json;
using System.Text.Json.Serialization;

namespace TestAutomationEngine.Core
{
    internal class UriConverter : JsonConverter<Uri>
    {
        public override Uri Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => new Uri(reader.GetString());
        public override void Write(Utf8JsonWriter writer, Uri value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString());
    }
}