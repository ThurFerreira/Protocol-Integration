using System.Runtime.Serialization;

namespace integra_dados.Models;

public enum WriteProtocol
{
    [EnumMember(Value = "LATCH_ON")] LATCH_ON,
    [EnumMember(Value = "LATCH_OFF")] LATCH_OFF,
    [EnumMember(Value = "PULSE_ON")] PULSE_ON,
    [EnumMember(Value = "PULSE_OFF")] PULSE_OFF
}