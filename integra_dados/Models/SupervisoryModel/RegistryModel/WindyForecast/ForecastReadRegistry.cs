namespace integra_dados.Models;

[BsonCollection("ForecastRegistry")]
public class ForecastReadRegistry : ReadRegistry
{
    public Location? Location { get; set; }
    public string? TipoMedida { get; set; }

    public ForecastReadRegistry()
    {
        Protocol = ReadProtocol.windy;
    }
}