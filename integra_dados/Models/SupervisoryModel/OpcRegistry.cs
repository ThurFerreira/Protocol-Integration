namespace integra_dados.Models.SupervisoryModel;

public class OpcRegistry : Registry
{
    public List<string> NodeAddress { get; set; }

    public string GetConnectionLink()
    {
        return "opc.tcp://" + Ip + ":" + Porta;
    }

}