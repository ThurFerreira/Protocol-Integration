namespace integra_dados.Models;

public class SupervisoryRegistry : Registry
{
    public string Id { get; set; }
    public int FreqLeituraSeg { get; set; }
    public string Protocolo { get; set; }

    // Dados de monitoramento do supervisório
    public string Ip { get; set; }
    public int Porta { get; set; }
    public int EnderecoInicio { get; set; }
    public int QuantidadeTags { get; set; }
}