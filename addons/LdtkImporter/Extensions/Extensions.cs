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
}