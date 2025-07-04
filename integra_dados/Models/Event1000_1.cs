namespace integra_dados.Models;

public class Event1000_1 : Event
{
   
    public float? ValorFloat { get; set; }
    public int? ValorInt { get; set; }
    public bool? ValorBool { get; set; }
    public string ValorString { get; set; }
    public float[] ValorArray { get; set; }
    
    public const int MaxTokenValue = 65535;

    public Event1000_1(int idSistema, string nome, int? valorInt, DateTime timeStamping) : base(idSistema, nome, timeStamping)
    {
        Uri = "1000/1";
        ValorInt = valorInt;
    }
    
    public Event1000_1(int idSistema, string nome, float? valorFloat, DateTime timeStamping) : base(idSistema, nome, timeStamping)
    {
        Uri = "1000/1";
        ValorFloat = valorFloat;
    }
    
    public Event1000_1(int idSistema, string nome, float[]? valorFloat, DateTime timeStamping) : base(idSistema, nome, timeStamping)
    {
        Uri = "1000/1";
        ValorArray = valorFloat;
    }
    
    public Event1000_1(int idSistema, string nome, string? valorString, DateTime timeStamping) : base(idSistema, nome, timeStamping)
    {
        Uri = "1000/1";
        ValorString = valorString;
    }
    
    public Event1000_1(int idSistema, string nome, bool? valorBool, DateTime timeStamping) : base(idSistema, nome, timeStamping)
    {
        Uri = "1000/1";
        ValorBool = valorBool;
    }
    
}