using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Resharper.Extensions;

public static class ObjectExtensions
{
    public static void Dump<T>(this T value, params string[] ignoredProperties) => Console.WriteLine(JsonSerializer.Serialize(value, new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        Converters = { new IgnorePropertyConverter<T>(ignoredProperties.ToHashSet()) }
    }));

    public static void Dump(this object value) => value.Dump(Array.Empty<string>());

    public static void Dump<T, TResult>(this T value, Func<T, TResult> selector) => selector(value).Dump();

    private class IgnorePropertyConverter<T> : JsonConverter<T>
    {
        private readonly HashSet<string> _ignoredProperties;
        public IgnorePropertyConverter(HashSet<string> ignoredProperties) => _ignoredProperties = ignoredProperties;
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (var property in typeof(T).GetProperties())
            {
                if (_ignoredProperties.Contains(property.Name)) continue;
                writer.WritePropertyName(property.Name);
                JsonSerializer.Serialize(writer, property.GetValue(value), property.PropertyType, options);
            }

            writer.WriteEndObject();
        }
    }
}