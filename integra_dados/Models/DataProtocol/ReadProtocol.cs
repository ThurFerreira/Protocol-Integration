using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace integra_dados.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReadProtocol
{
    //annotation for enum be written as string
    [EnumMember(Value = "modbus")] modbus,
    [EnumMember(Value = "opcua")] opcua,
    [EnumMember(Value = "dnp3")] dnp3,
    [EnumMember(Value = "bacnet")] bacnet,
    [EnumMember(Value = "windy")] windy
}