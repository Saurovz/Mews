using System.Runtime.Serialization;

namespace TaxManager.Domain.Enums;

public enum Strategy
{
    [EnumMember(Value = "Flat Rate")]
    FlatRate = 1,
    [EnumMember(Value = "Relative Rate")]
    RelativeRate,
    [EnumMember(Value = "Relative Rate With Dependencies")]
    RelativeRateWithDependencies
}
