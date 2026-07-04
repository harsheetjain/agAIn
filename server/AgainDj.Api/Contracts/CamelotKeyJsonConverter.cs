using System.Text.Json;
using System.Text.Json.Serialization;
using AgainDj.Domain.Model;

namespace AgainDj.Api.Contracts;

/// <summary>Serializes <see cref="CamelotKey"/> as its compact string form (e.g. "8A").</summary>
public sealed class CamelotKeyJsonConverter : JsonConverter<CamelotKey>
{
    public override CamelotKey Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        CamelotKey.Parse(reader.GetString());

    public override void Write(Utf8JsonWriter writer, CamelotKey value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString());
}
