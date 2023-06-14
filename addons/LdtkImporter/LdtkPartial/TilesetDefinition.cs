using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using Godot.Collections;

namespace LdtkImporter;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public partial class TilesetDefinition : IImporter, IJsonOnDeserialized
{
    [JsonIgnore] public TileSet TileSet { get; set; }
    [JsonIgnore] public string JsonString { get; set; }

    public Error PreImport(LdtkJson ldtkJson, string savePath, Dictionary options, Array<string> genFiles)
    {
        if (EmbedAtlas != null)
        {
            GD.Print($"   is embedAtlas:{Identifier}, ignore.");
            return Error.Ok;
        }
        GD.Print($"  {Identifier}");

        var key = $"{LdtkImporterPlugin.OptionTilesetMapping}/{Identifier}";
        var tileSetPath = options.GetValueOrDefault<string>(key);
        var prefix2Remove = options.GetValueOrDefault<string>(LdtkImporterPlugin.OptionGeneralPrefix2Remove);
        var prefix2Add = options.GetValueOrDefault<string>(LdtkImporterPlugin.OptionGeneralPrefix2Add);

        if (string.IsNullOrWhiteSpace(tileSetPath))
        {
            GD.Print($"   property:{key} is not set.");
            return Error.FileNotFound;
        }

        var createTileSet = options.GetValueOrDefault<bool>(LdtkImporterPlugin.OptionTilesetCreate);
        if (!ResourceLoader.Exists(tileSetPath))
        {
            if (!createTileSet)
            {
                GD.Print($"  tileset:{tileSetPath} is not exist.");
                return Error.FileNotFound;
            }

            GenerateTileSet(ldtkJson, options);
        }

        var tileSetLoad = ResourceLoader.Load<TileSet>(tileSetPath);
        if (tileSetLoad == null)
        {
            GD.Print($"  {tileSetPath} is not a godot tileset resource.");
            return Error.Failed;
        }

        TileSet = tileSetLoad;
        TileSet.AddCustomDataLayerIfNotExist($"{prefix2Add}{Identifier}", Variant.Type.Dictionary);
        TileSet.RemoveMetaPrefix(prefix2Remove);

        GD.Print($"   load godot tileset success:{tileSetPath}");

        return Error.Ok;
    }

    public Error Import(LdtkJson ldtkJson, string savePath, Dictionary options, Array<string> genFiles)
    {
        if (EmbedAtlas != null)
        {
            GD.Print($"   is embedAtlas:{Identifier}, ignore.");
            return Error.Ok;
        }
        GD.Print($"  {Identifier}:{TileSet.ResourcePath}");

        var key = $"{LdtkImporterPlugin.OptionTilesetMapping}/{Identifier}";
        var tileSetPath = options.GetValueOrDefault<string>(key);
        var prefix2Add = options.GetValueOrDefault<string>(LdtkImporterPlugin.OptionGeneralPrefix2Add);
        var customLayerName = $"{prefix2Add}{Identifier}";
        var sourceId = TileSet.GetSourceIdByName(customLayerName);
        if (sourceId == -1)
        {
            GD.Print($"   none TileSetAtlasSource named:{customLayerName} in {tileSetPath}, named it, and try again.");
            return Error.Failed;
        }

        var customDataLayerIndex = TileSet.GetCustomDataLayerByName(customLayerName);

        if (options.GetValueOrDefault<bool>(LdtkImporterPlugin.OptionTilesetImportTileCustomData))
        {
            var meta = Json.ParseString(JsonString);
            TileSet.SetMeta($"{prefix2Add}tilesets", meta);
        }

        var source = (TileSetAtlasSource)TileSet.GetSource(sourceId);
        var importTileCustomData = options.GetValueOrDefault<bool>(key);
        if (importTileCustomData)
        {
            foreach (var customMetadata in CustomData)
            {
                var atlasCoords = customMetadata.TileId.AtlasCoords(source);
                var tileData = source.GetTileData(atlasCoords, 0);
                var data = Json.ParseString(customMetadata.Data);

                tileData.SetCustomDataByLayerId(customDataLayerIndex, data);

                GD.Print($"   tileId:{customMetadata.TileId}/{atlasCoords}, data:{data}");
            }
        }
        else
        {
            GD.Print($"   {key} is false, skip import custom data to godot TileData.");
        }

        return Error.Ok;
    }

    public Error PostImport(LdtkJson ldtkJson, string savePath, Dictionary options, Array<string> genFiles)
    {
        if (EmbedAtlas != null)
        {
            GD.Print($"   is embedAtlas:{Identifier}, ignore.");
            return Error.Ok;
        }
        GD.Print($"  save tileset:{TileSet.ResourcePath}");

        ResourceSaver.Save(TileSet, TileSet.ResourcePath);

        genFiles.Add(TileSet.ResourcePath);
        return Error.Ok;
    }

    public void OnDeserialized()
    {
        JsonString = JsonSerializer.Serialize(this);
    }

    public void GenerateTileSet(LdtkJson ldtkJson, Dictionary options)
    {
        var key = $"{LdtkImporterPlugin.OptionTilesetMapping}/{Identifier}";
        var tileSetPath = options.GetValueOrDefault<string>(key);
        var prefix2Add = options.GetValueOrDefault<string>(LdtkImporterPlugin.OptionGeneralPrefix2Add);
        var imagePath = ldtkJson.Path.GetBaseDir().PathJoin(RelPath);
        var tileSet = new TileSet();
        var texture2D = ResourceLoader.Load<Texture2D>(imagePath);
        var source = new TileSetAtlasSource();

        tileSet.TileSize = new Vector2I((int)TileGridSize, (int)TileGridSize);
        tileSet.ResourcePath = tileSetPath;
        tileSet.AddSource(source, (int)Uid);

        source.ResourceName = $"{prefix2Add}{Identifier}";
        source.Margins = new Vector2I((int)Padding, (int)Padding);
        source.Separation = new Vector2I((int)Spacing, (int)Spacing);
        source.Texture = texture2D;
        source.TextureRegionSize = new Vector2I((int)TileGridSize, (int)TileGridSize);

        var gridWidth = (PxWid - 2 * Padding + Spacing) / (TileGridSize + Spacing);
        var gridHeight = (PxHei - 2 * Padding + Spacing) / (TileGridSize + Spacing);

        for (var y = 0; y < gridHeight; y++)
        {
            for (var x = 0; x < gridWidth; x++)
            {
                var gridCoords = new Vector2I(x, y);
                source.CreateTile(gridCoords);
            }
        }

        DirAccess.MakeDirRecursiveAbsolute(tileSetPath.GetBaseDir());

        ResourceSaver.Save(tileSet, tileSetPath);
    }
}