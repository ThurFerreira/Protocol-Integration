using integra_dados.Models.DataProtocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace integra_dados.Models;

public class Event1000_2 : Event
{
    [JsonConverter(typeof(StringEnumConverter))]
    public WriteProtocol WriteProtocol { get; set; }
    [JsonConverter(typeof(StringEnumConverter))]
    public Protocol Protocol { get; set; }

    public Event1000_2(int idSistema, string nome, DateTime timeStamping, WriteProtocol writeReadProtocol) : base(idSistema, nome, timeStamping)
    {
        Uri = "1000/2";
        WriteProtocol = writeReadProtocol;
    }

    public Event1000_2()
    {
    }
}