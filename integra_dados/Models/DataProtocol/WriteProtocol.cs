using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace integra_dados.Models.DataProtocol;

[System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
public enum WriteProtocol
{
    [EnumMember(Value = "LATCH_ON")] LATCH_ON,
    [EnumMember(Value = "LATCH_OFF")] LATCH_OFF,
    [EnumMember(Value = "PULSE_ON")] PULSE_ON,
    [EnumMember(Value = "PULSE_OFF")] PULSE_OFF
}