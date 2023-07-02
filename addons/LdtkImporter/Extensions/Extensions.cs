using System;
using System.Linq;
using Godot;

namespace LdtkImporter;

public static class Extensions
{
    public static AlternativeIdFlags PivotXFlags(this float pivotX)
    {
        if (Math.Abs(pivotX - 0.25) < 0.01f)
        {
            return AlternativeIdFlags.PivotXOneQuarter;
        }

        if (Math.Abs(pivotX - 0.5) < 0.01f)
        {
            return AlternativeIdFlags.PivotXHalf;
        }

        if (Math.Abs(pivotX - 0.75) < 0.01f)
        {
            return AlternativeIdFlags.PivotXThreeQuarter;
        }

        return AlternativeIdFlags.None;
    }

    public static AlternativeIdFlags PivotYFlags(this float pivotY)
    {
        if (Math.Abs(pivotY - 0.25) < 0.01f)
        {
            return AlternativeIdFlags.PivotYOneQuarter;
        }

        if (Math.Abs(pivotY - 0.5) < 0.01f)
        {
            return AlternativeIdFlags.PivotYHalf;
        }

        if (Math.Abs(pivotY - 0.75) < 0.01f)
        {
            return AlternativeIdFlags.PivotYThreeQuarter;
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
        var (x, _) = tileSetAtlasSource.GetAtlasGridSize();
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