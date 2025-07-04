namespace integra_dados.Models.SupervisoryModel.RegistryModel.OPCUA;

[BsonCollection("OpcRegistry")]
public class OpcReadRegistry : ReadRegistry
{
    public List<string> NodeAddress { get; set; }

    public string GetConnectionLink()
    {
        return "opc.tcp://" + Ip + ":" + Porta;
    }

}