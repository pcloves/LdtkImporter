using Godot;
using Godot.Collections;

namespace Ldtk;

public partial class LdtkJson
{
    public string Path { get; set; }

    public static LdtkJson FromPath(string path)
    {
        var baseDir = path.GetBaseDir();
        var json = FileAccess.Open(path, FileAccess.ModeFlags.Read).GetAsText();
        var ldtkJson = FromJson(json);

        ldtkJson.Path = path;

        foreach (var tileset in ldtkJson.Defs.Tilesets)
        {
            tileset.ImagePath = baseDir.PathJoin(tileset.RelPath);
        }

        return ldtkJson;
    }
}

public partial class TilesetDefinition
{
    public string ImagePath { get; set; }

    public TileSet GenerateTileSet(Dictionary options)
    {
        var tileSet = new TileSet();
        var texture2D = ResourceLoader.Load<Texture2D>(ImagePath);

        tileSet.TileSize = new Vector2I(TileGridSize, TileGridSize);
        tileSet.ResourceName = ImagePath.GetBaseName().GetFile();

        var source = new TileSetAtlasSource();

        source.Margins = new Vector2I((int)Padding, (int)Padding);
        source.Separation = new Vector2I((int)Spacing, (int)Spacing);
        source.Texture = texture2D;
        source.TextureRegionSize = new Vector2I(TileGridSize, TileGridSize);

        var gridWidth = (PxWid - 2 * Padding + Spacing) / (TileGridSize + Spacing);
        var gridHeight = (PxHei - 2 * Padding + Spacing) / (TileGridSize + Spacing);

        for (var y = 0; y < gridHeight; y++)
        {
            for (var x = 0; x < gridWidth; x++)
            {
                var gridCoords = new Vector2I(x, y);
                source.CreateTile(gridCoords, tileSet.TileSize);
            }
        }


        return tileSet;
    }
}