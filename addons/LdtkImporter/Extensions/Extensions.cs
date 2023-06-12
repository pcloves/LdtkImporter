using System.Linq;
using Godot;

namespace LdtkImporter;

public static class Extensions
{
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

    public static bool IsValidPrefix(this string prefix)
    {
        //https://github.com/godotengine/godot/blob/2d6b880987bc600cda586b281fcbe26791e92e09/core/string/ustring.cpp#LL3954C1-L3954C1
        return prefix.Length == 0 ||
               (prefix[0] < '0' || prefix[0] > '9' && prefix.All(c => char.IsLetterOrDigit(c) || c == '_'));
    }
}