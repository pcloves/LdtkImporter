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
        GD.Print($"  {Identifier}");
        if (EmbedAtlas != null)
        {
            GD.Print($"   is embedAtlas:{Identifier}, ignore.");
            return Error.Ok;
        }

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

            TileSet = new TileSet();
            TileSet.TileSize = new Vector2I((int)TileGridSize, (int)TileGridSize);
            //TODO:增加图集
        }

        var tileSetGodot = ResourceLoader.Load<TileSet>(tileSetPath);
        if (tileSetGodot == null)
        {
            GD.Print($"  {tileSetPath} is not a godot tileset resource.");
            return Error.Failed;
        }

        TileSet = tileSetGodot;
        TileSet.AddCustomDataLayerIfNotExist($"{prefix2Add}{Identifier}", Variant.Type.Dictionary);
        TileSet.RemoveMetaPrefix(prefix2Remove);

        GD.Print($"   load godot tileset success:{tileSetPath}");

        return Error.Ok;
    }

    public Error Import(LdtkJson ldtkJson, string savePath, Dictionary options, Array<string> genFiles)
    {
        GD.Print($"  {Identifier}:{TileSet.ResourcePath}");

        if (TileSet.GetSourceCount() == 0)
        {
            GD.Print($"   none source exist.");
            return Error.Failed;
        }

        var prefix2Add = options.GetValueOrDefault<string>(LdtkImporterPlugin.OptionGeneralPrefix2Add);
        var customLayerName = $"{prefix2Add}{Identifier}";
        var customDataLayerIndex = TileSet.GetCustomDataLayerByName(customLayerName);

        if (options.GetValueOrDefault<bool>(LdtkImporterPlugin.OptionTilesetImportTileCustomData))
        {
            var meta = Json.ParseString(JsonString);
            TileSet.SetMeta($"{prefix2Add}tilesets", meta);
        }

        //TODO:这里用0有些武断了
        var sourceId = TileSet.GetSourceId(0);
        var sourceIdNew = (int)Uid;

        //TODO:而且这里强行更改sourceId也有些武断了
        TileSet.SetSourceId(sourceId, sourceIdNew);
        GD.Print($"   update the first source id:{sourceId} -> {Uid}");

        var source = (TileSetAtlasSource)TileSet.GetSource(sourceIdNew);
        const string key = LdtkImporterPlugin.OptionTilesetImportTileCustomData;
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
        GD.Print($"  save tileset:{TileSet.ResourcePath}");
        ResourceSaver.Save(TileSet, TileSet.ResourcePath);

        genFiles.Add(TileSet.ResourcePath);
        return Error.Ok;
    }

    public void OnDeserialized()
    {
        JsonString = JsonSerializer.Serialize(this);
    }
}