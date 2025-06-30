using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace integra_dados.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Protocol
{
    //annotation for enum can be written as string
    [EnumMember(Value = "dnp3")] dnp3,

    [EnumMember(Value = "modbus")] modbus,

    [EnumMember(Value = "opcua")] opcua,

    [EnumMember(Value = "windy")] windy
}