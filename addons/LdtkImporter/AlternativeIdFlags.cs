using System;
using System.Diagnostics.CodeAnalysis;

namespace LdtkImporter;

[Flags]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum AlternativeIdFlags
{
    None = 0,

    //X flip only
    FlipH = 1 << 0,

    //Y flip only
    FlipV = 1 << 1,

    //Pivot X 0.1
    PivotXOneTenth = 1 << 2,

    //Pivot X 0.2
    PivotXTwoTenths = 1 << 3,

    //Pivot X 0.3
    PivotXThreeTenths = 1 << 4,

    //Pivot X 0.4
    PivotXFourTenths = 1 << 5,

    //Pivot X 0.5
    PivotXFiveTenths = 1 << 6,

    //Pivot X 0.6
    PivotXSixTenths = 1 << 7,

    //Pivot X 0.7
    PivotXSevenTenths = 1 << 8,

    //Pivot X 0.8
    PivotXEightTenths = 1 << 9,

    //Pivot X 0.9
    PivotXNightTenths = 1 << 10,

    //Pivot Y 0.1
    PivotYOneTenth = 1 << 11,

    //Pivot Y 0.2
    PivotYTwoTenths = 1 << 12,

    //Pivot Y 0.3
    PivotYThreeTenths = 1 << 13,

    //Pivot Y 0.4
    PivotYFourTenths = 1 << 14,

    //Pivot Y 0.5
    PivotYFiveTenths = 1 << 15,

    //Pivot Y 0.6
    PivotYSixTenths = 1 << 16,

    //Pivot Y 0.7
    PivotYSevenTenths = 1 << 17,

    //Pivot Y 0.8
    PivotYEightTenths = 1 << 18,

    //Pivot Y 0.9
    PivotYNightTenths = 1 << 19,
}