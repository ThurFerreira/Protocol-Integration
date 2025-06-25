namespace integra_dados.Models;

[BsonCollection("ForecastRegistry")]
public class ForecastRegistry : Registry
{
    public Location Location { get; set; }
    public string? TipoMedida { get; set; }
}