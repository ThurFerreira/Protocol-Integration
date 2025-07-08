using System.IO.BACnet;

namespace integra_dados.Models.SupervisoryModel.RegistryModel.BACnet;

[BsonCollection("bacnetRegistries")]
public class BACnetRegistry: Registry
{
    // OBJECT_ANALOG_INPUT = 0
    // OBJECT_ANALOG_OUTPUT = 1
    // OBJECT_BINARY_INPUT = 3
    // OBJECT_BINARY_OUTPUT = 4
    public BacnetObjectTypes BacnetObjectTypes { get; set; }
    public int Flag { get; set; }
}