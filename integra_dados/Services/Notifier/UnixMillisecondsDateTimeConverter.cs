using System.Text.Json;
using System.Text.Json.Serialization;

namespace integra_dados.Services.Notifier;

public class UnixMillisecondsDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Lê o número como Unix timestamp em milissegundos
        long milliseconds = reader.GetInt64();
        return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).UtcDateTime;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Escreve o DateTime como Unix timestamp em milissegundos
        long milliseconds = new DateTimeOffset(value).ToUnixTimeMilliseconds();
        writer.WriteNumberValue(milliseconds);
    }
}