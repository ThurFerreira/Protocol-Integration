using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace integra_dados.Util;

public class SimpleJsonDeserializer<T> : IDeserializer<T>
{
    public T? Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        var json = System.Text.Encoding.UTF8.GetString(data);
        // var json = JsonConvert.DeserializeObject<string>(rawString);
        return JsonConvert.DeserializeObject<T>(json);
    }
}