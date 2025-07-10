using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace integra_dados.Models.DataProtocol;

[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]public enum Protocol
{
    //annotation for enum be written as string
    [EnumMember(Value = "modbus")] modbus,
    [EnumMember(Value = "opcua")] opcua,
    [EnumMember(Value = "dnp3")] dnp3,
    [EnumMember(Value = "bacnet")] bacnet,
    [EnumMember(Value = "windy")] windy
}