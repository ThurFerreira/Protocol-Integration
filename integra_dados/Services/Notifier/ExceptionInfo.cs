namespace integra_dados.Services.Notifier;

public class ExceptionInfo
{
    public int Light { get; set; } = 0;
    public int Moderate { get; set; }  = 0;
    public int Critical { get; set; }  = 0;

    public void IncrementLight()
    {
        Light++;
    }

    public void IncrementModerate()
    {
        Moderate++;
    }

    public void IncrementCritical()
    {
        Critical++;
    }
}