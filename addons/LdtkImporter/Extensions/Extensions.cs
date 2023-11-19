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
        if (pivotX < 0.05)
        {
            return AlternativeIdFlags.None;
        }

        if (pivotX >= 0.05 && pivotX < 0.15)
        {
            return AlternativeIdFlags.PivotXOneTenth;
        }

        if (pivotX >= 0.15 && pivotX < 0.25)
        {
            return AlternativeIdFlags.PivotXTwoTenths;
        }

        if (pivotX >= 0.25 && pivotX < 0.35)
        {
            return AlternativeIdFlags.PivotXThreeTenths;
        }

        if (pivotX >= 0.35 && pivotX < 0.45)
        {
            return AlternativeIdFlags.PivotXFourTenths;
        }

        if (pivotX >= 0.45 && pivotX < 0.55)
        {
            return AlternativeIdFlags.PivotXFiveTenths;
        }

        if (pivotX >= 0.55 && pivotX < 0.65)
        {
            return AlternativeIdFlags.PivotXSixTenths;
        }

        if (pivotX >= 0.65 && pivotX < 0.75)
        {
            return AlternativeIdFlags.PivotXSevenTenths;
        }

        if (pivotX >= 0.75 && pivotX < 0.85)
        {
            return AlternativeIdFlags.PivotXEightTenths;
        }

        if (pivotX >= 0.85 && pivotX < 0.95)
        {
            return AlternativeIdFlags.PivotXNightTenths;
        }

        return AlternativeIdFlags.None;
    }

    public static AlternativeIdFlags PivotYFlags(this float pivotY)
    {
        if (pivotY < 0.05)
        {
            return AlternativeIdFlags.None;
        }

        if (pivotY >= 0.05 && pivotY < 0.15)
        {
            return AlternativeIdFlags.PivotYOneTenth;
        }

        if (pivotY >= 0.15 && pivotY < 0.25)
        {
            return AlternativeIdFlags.PivotYTwoTenths;
        }

        if (pivotY >= 0.25 && pivotY < 0.35)
        {
            return AlternativeIdFlags.PivotYThreeTenths;
        }

        if (pivotY >= 0.35 && pivotY < 0.45)
        {
            return AlternativeIdFlags.PivotYFourTenths;
        }

        if (pivotY >= 0.45 && pivotY < 0.55)
        {
            return AlternativeIdFlags.PivotYFiveTenths;
        }

        if (pivotY >= 0.55 && pivotY < 0.65)
        {
            return AlternativeIdFlags.PivotYSixTenths;
        }

        if (pivotY >= 0.65 && pivotY < 0.75)
        {
            return AlternativeIdFlags.PivotYSevenTenths;
        }

        if (pivotY >= 0.75 && pivotY < 0.85)
        {
            return AlternativeIdFlags.PivotYEightTenths;
        }

        if (pivotY >= 0.85 && pivotY < 0.95)
        {
            return AlternativeIdFlags.PivotYNightTenths;
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