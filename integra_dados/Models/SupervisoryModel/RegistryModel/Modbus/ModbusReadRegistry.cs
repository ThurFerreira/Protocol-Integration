namespace integra_dados.Models.SupervisoryModel.RegistryModel.Modbus;

// [BsonCollection("ModbusRegistry")]
[BsonCollection("SupervisoryRegistry")]
public class ModbusReadRegistry : ReadRegistry
{
    // Dados de monitoramento do supervisório
    public int EnderecoInicio { get; set; } //TODO mudar para int
    public int QuantidadeTags { get; set; }
}