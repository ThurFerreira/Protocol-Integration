namespace integra_dados.Models;

[BsonCollection("ForecastRegistry")]
public class ForecastRegistry : Registry
{
    public double Lat { get; set; }
    public double Lng { get; set; }
}