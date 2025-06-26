namespace integra_dados.Models;

[BsonCollection("SupervisoryRegistry")]
public class ModbusRegistry : Registry
{
    public string? Protocolo { get; set; }

    // Dados de monitoramento do supervisório
    public string? Ip { get; set; }
    public int Porta { get; set; } //TODO int
    public int EnderecoInicio { get; set; } //TODO mudar para int
    public int QuantidadeTags { get; set; }
}