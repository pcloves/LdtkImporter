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
    
    //Pivot X 0.2
    PivotXTwoTenths = 1 << 2,
    //Pivot X 0.4
    PivotXFourTenths = 1 << 3,
    //Pivot X 0.6
    PivotXSixTenths = 1 << 4,
    //Pivot X 0.8
    PivotXEightTenths = 1 << 5,
    
    //Pivot Y 0.2
    PivotYTwoTenths = 1 << 6,
    //Pivot Y 0.4
    PivotYFourTenths = 1 << 7,
    //Pivot Y 0.6
    PivotYSixTenths = 1 << 8,
    //Pivot Y 0.8
    PivotYEightTenths = 1 << 9,
    
    //12, 13, 14已经被godot系统占用
    //https://github.com/godotengine/godot/blob/fa1fb2a53e20a3aec1ed1ffcc516f880f74db1a6/scene/resources/tile_set.h#L602
    TRANSFORM_FLIP_H = 1 << 12,
    TRANSFORM_FLIP_V = 1 << 13,
    TRANSFORM_TRANSPOSE = 1 << 14,
    
    //godot中使用int_16存储alternative id，所以这里不能超过16位
}