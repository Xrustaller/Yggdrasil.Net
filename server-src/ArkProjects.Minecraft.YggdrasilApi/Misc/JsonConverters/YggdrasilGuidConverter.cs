using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArkProjects.Minecraft.YggdrasilApi.Misc.JsonConverters;

public class YggdrasilGuidConverter : JsonConverter<Guid>
{
    // запись без дефисов
    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("N"));
    }

    // чтение guid из строки
    public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? str = reader.GetString();
        return str != null ? Guid.Parse(str) : Guid.Empty;
    }
}