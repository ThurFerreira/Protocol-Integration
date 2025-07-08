using Newtonsoft.Json;

namespace integra_dados.Models;

public class Event
{
    public string Uri { get; set; }
    public int IdSistema { get; set; }
    public string Nome { get; set; }
    public int Token { get; set; } = 0;
    public DateTime? TimeStamping { get; set; }

    public Event(int idSistema, string nome, DateTime timeStamping)
    {
        IdSistema = idSistema;
        Nome = nome;
        TimeStamping = timeStamping;
    }

    public Event()
    {
    }
}