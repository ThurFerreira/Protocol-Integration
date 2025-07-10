namespace integra_dados.Models.SupervisoryModel.RegistryModel.WindyForecast;

[BsonCollection("windyForecastRegistries")]
public class ForecastRegistry : Registry
{
    public Location? Location { get; set; }
    public string? TipoMedida { get; set; }

    public ForecastRegistry()
    {
        Protocol = DataProtocol.Protocol.windy;
    }
}