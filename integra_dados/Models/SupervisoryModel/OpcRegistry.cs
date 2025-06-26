namespace integra_dados.Models.SupervisoryModel;

public class OpcRegistry : Registry
{
    public string LinkConexao { get; set; }
    public List<string> NodeAddress { get; set; }

}