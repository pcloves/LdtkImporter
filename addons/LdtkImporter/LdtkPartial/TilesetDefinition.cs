using System.Collections.Generic;
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
        var tileSetPath = options.GetValueOrDefault(key).AsString().Trim();

        if (string.IsNullOrWhiteSpace(tileSetPath))
        {
            GD.Print($"   property:{key} is not set.");
            return Error.FileNotFound;
        }

        if (!ResourceLoader.Exists(tileSetPath))
        {
            GD.Print($"  tileset:{tileSetPath} is not exist.");
            return Error.FileNotFound;
        }

        var tileSetGodot = ResourceLoader.Load<TileSet>(tileSetPath);
        if (tileSetGodot == null)
        {
            GD.Print($"  {tileSetPath} is not a godot tileset resource.");
            return Error.Failed;
        }

        TileSet = tileSetGodot;

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

        if (TileSet.GetCustomDataLayersCount() == 0)
        {
            GD.Print($"   none custom data layer exist, add one.");
            TileSet.AddCustomDataLayer();
        }

        if (options.GetValueOrDefault(LdtkImporterPlugin.OptionTilesetImportTileCustomData).AsBool())
        {
            var meta = Json.ParseString(JsonString);
            TileSet.SetCustomDataLayerName(0, Identifier);
            TileSet.SetCustomDataLayerType(0, Variant.Type.Dictionary);
            TileSet.SetMeta("tilesets", meta);
        }

        var sourceId = TileSet.GetSourceId(0);
        var sourceIdNew = (int)Uid;

        TileSet.SetSourceId(sourceId, sourceIdNew);
        GD.Print($"   update the first source id:{sourceId} -> {Uid}");

        var source = (TileSetAtlasSource)TileSet.GetSource(sourceIdNew);
        const string key = LdtkImporterPlugin.OptionTilesetImportTileCustomData;
        var importTileCustomData = options.GetValueOrDefault(key).AsBool();
        if (importTileCustomData)
        {
            foreach (var customMetadata in CustomData)
            {
                var atlasCoords = customMetadata.TileId.AtlasCoords(source);
                var tileData = source.GetTileData(atlasCoords, 0);
                var data = Json.ParseString(customMetadata.Data);
                tileData.SetCustomDataByLayerId(0, data);

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