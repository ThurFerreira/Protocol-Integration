namespace integra_dados.Models;

[BsonCollection("SupervisoryRegistry")]
public class SupervisoryRegistry : Registry
{
    public string? FreqLeituraSeg { get; set; } //TODO int
    public string? Protocolo { get; set; }

    // Dados de monitoramento do supervisório
    public string? Ip { get; set; }
    public string? Porta { get; set; } //TODO int
    public string? EnderecoInicio { get; set; } //TODO mudar para int
    public int QuantidadeTags { get; set; }
}