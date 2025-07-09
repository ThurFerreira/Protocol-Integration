using System.IO.BACnet;

namespace integra_dados.Models.SupervisoryModel.RegistryModel.BACnet;

[BsonCollection("bacnetRegistries")]
public class BACnetRegistry: Registry
{
    // OBJECT_ANALOG_INPUT = 0
    // OBJECT_BINARY_INPUT = 3
    public BacnetObjectTypes BacnetObjectTypes { get; set; }
    public uint Instance { get; set; }
}