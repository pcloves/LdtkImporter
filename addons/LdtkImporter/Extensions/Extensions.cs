using System;
using System.Linq;
using Godot;

namespace LdtkImporter;

public static class Extensions
{
    public static T ToEnum<T>(this string value)
    {
        return (T) Enum.Parse(typeof(T), value, true);
    }
    
    public static AlternativeIdFlags PivotXFlags(this float pivotX)
    {
        if (pivotX < 0.2)
        {
            return AlternativeIdFlags.None;
        }

        if (pivotX >= 0.2 && pivotX < 0.4)
        {
            return AlternativeIdFlags.PivotXTwoTenths;
        }

        if (pivotX >= 0.4 && pivotX < 0.6)
        {
            return AlternativeIdFlags.PivotXFourTenths;
        }

        if (pivotX >= 0.6 && pivotX < 0.8)
        {
            return AlternativeIdFlags.PivotXSixTenths;
        }

        if (pivotX >= 0.8 && pivotX < 1.0)
        {
            return AlternativeIdFlags.PivotXEightTenths;
        }
        
        return AlternativeIdFlags.None;
    }

    public static AlternativeIdFlags PivotYFlags(this float pivotY)
    {
        if (pivotY < 0.2)
        {
            return AlternativeIdFlags.None;
        }

        if (pivotY >= 0.2 && pivotY < 0.4)
        {
            return AlternativeIdFlags.PivotYTwoTenths;
        }

        if (pivotY >= 0.4 && pivotY < 0.6)
        {
            return AlternativeIdFlags.PivotYFourTenths;
        }

        if (pivotY >= 0.6 && pivotY < 0.8)
        {
            return AlternativeIdFlags.PivotYSixTenths;
        }

        if (pivotY >= 0.8 && pivotY < 1.0)
        {
            return AlternativeIdFlags.PivotYEightTenths;
        }
        
        return AlternativeIdFlags.None;
    }

    public static Vector2I AtlasCoords(this int tileId, TileSetAtlasSource tileSetAtlasSource)
    {
        var tileIdLong = (long)tileId;
        return tileIdLong.AtlasCoords(tileSetAtlasSource);
    }

    public static Vector2I AtlasCoords(this long tileId, TileSetAtlasSource tileSetAtlasSource)
    {
        var (x, y) = tileSetAtlasSource.GetAtlasGridSize();
        return new Vector2I((int)(tileId % x), (int)(tileId / x));
    }

    public static long TileId(this Vector2I atlasCoords, TileSetAtlasSource tileSetAtlasSource)
    {
        var (x, _) = tileSetAtlasSource.GetAtlasGridSize();
        return atlasCoords.Y * x + atlasCoords.X;
    }

    public static Vector2I AtlasCoords(this Vector2I px, TileSetAtlasSource source)
    {
        var atlasCoords = (px - source.Margins) / (source.TextureRegionSize + source.Separation);
        return atlasCoords;
    }

    public static bool IsValidPrefix(this string prefix)
    {
        //https://github.com/godotengine/godot/blob/2d6b880987bc600cda586b281fcbe26791e92e09/core/string/ustring.cpp#LL3954C1-L3954C1
        return prefix.Length == 0 ||
               (prefix[0] < '0' || prefix[0] > '9' && prefix.All(c => char.IsLetterOrDigit(c) || c == '_'));
    }
}