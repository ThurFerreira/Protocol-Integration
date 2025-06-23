namespace integra_dados.Models;

public class Event1000_1
{
    public string Uri { get; set; } = "1000/1";
    public int IdSistema { get; set; }
    public string Nome { get; set; }
    public int Token { get; set; } = 0;
    public float ValorFloat { get; set; }
    public int ValorInt { get; set; }
    public bool ValorBool { get; set; }
    public string ValorString { get; set; }
    public float[] ValorArray { get; set; }

    public DateTime TimeStamping { get; set; }

    public const int MaxTokenValue = 65535;

    public Event1000_1(int idSistema, string nome, int token, int valorInt, DateTime timeStamping)
    {
        IdSistema = idSistema;
        Nome = nome;
        Token = token;
        ValorInt = valorInt;
        TimeStamping = timeStamping;
    }
    
    public Event1000_1(int idSistema, string nome, int token, float valorFloat, DateTime timeStamping)
    {
        IdSistema = idSistema;
        Nome = nome;
        Token = token;
        ValorFloat = valorFloat;
        TimeStamping = timeStamping;
    }
    
    public Event1000_1(int idSistema, string nome, int token, float[] valorFloat, DateTime timeStamping)
    {
        IdSistema = idSistema;
        Nome = nome;
        Token = token;
        ValorArray = valorFloat;
        TimeStamping = timeStamping;
    }
    
    public Event1000_1(int idSistema, string nome, int token, string valorString, DateTime timeStamping)
    {
        IdSistema = idSistema;
        Nome = nome;
        Token = token;
        ValorString = valorString;
        TimeStamping = timeStamping;
    }
    
    public Event1000_1(int idSistema, string nome, int token, bool valorBool, DateTime timeStamping)
    {
        IdSistema = idSistema;
        Nome = nome;
        Token = token;
        ValorBool = valorBool;
        TimeStamping = timeStamping;
    }
    
}