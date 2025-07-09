using Newtonsoft.Json;

namespace integra_dados.Models;

public class Event1000_2 : Event
{
    public WriteProtocol WriteProtocol { get; set; }
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