using System.Text.Json;
using System.Text.Json.Serialization;

namespace SportMap.API.Middleware;

public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => TimeOnly.Parse(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString("HH:mm"));
}