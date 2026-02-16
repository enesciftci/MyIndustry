using System.ComponentModel;

namespace MyIndustry.Domain.ValueObjects;

public enum SellerSector
{
    [Description("Hırdavat")]
    IronMongery = 1,
    [Description("Damper")]
    Dumper = 2
}