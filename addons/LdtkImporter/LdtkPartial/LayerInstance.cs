using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using Godot.Collections;

namespace LdtkImporter;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public partial class LayerInstance : IImporter, IJsonOnDeserialized
{
    [JsonIgnore] public string JsonString { get; set; }
    [JsonIgnore] public Node2D Root { get; set; }

    private static readonly System.Collections.Generic.Dictionary<string, Func<Node2D>> TypeMap = new()
    {
        { nameof(TypeEnum.IntGrid), () => new TileMap() },
        { nameof(TypeEnum.AutoLayer), () => new TileMap() },
        { nameof(TypeEnum.Tiles), () => new TileMap() },
        { nameof(TypeEnum.Entities), () => new Node2D() },
    };

    public Error PreImport(LdtkJson ldtkJson, string savePath, Dictionary options, Array<string> genFiles)
    {
        if (TypeMap.TryGetValue(Type, out var func) == false)
        {
            GD.Print($"    invalid LayerInstance type:{Type}");
            return Error.Failed;
        }

        Root = func.Invoke();
        Root.Name = Identifier;
        Root.Position = new Vector2(PxTotalOffsetX, PxTotalOffsetY);
        Root.SetMeta("instance", Json.ParseString(JsonString));

        var tileMap = Root as TileMap;

        var tilesetDefinition = ldtkJson.Defs.Tilesets.FirstOrDefault(definition => definition.Uid == TilesetDefUid);
        if (tileMap != null)
        {
            tileMap.TileSet = tilesetDefinition?.TileSet;
            tileMap.CellQuadrantSize = (int)GridSize;
        }

        return Error.Ok;
    }

    public Error Import(LdtkJson ldtkJson, string savePath, Dictionary options, Array<string> genFiles)
    {
        Error error;
        switch (Type)
        {
            case nameof(TypeEnum.IntGrid):
            case nameof(TypeEnum.AutoLayer):
            case nameof(TypeEnum.Tiles):
                error = ImportTile(ldtkJson, options);
                break;
            case nameof(TypeEnum.Entities):
                error = ImportEntity(ldtkJson);
                break;
            default:
                error = Error.Bug;
                break;
        }

        return error;
    }

    public Error PostImport(LdtkJson ldtkJson, string savePath, Dictionary options, Array<string> genFiles)
    {
        return Error.Ok;
    }

    public void OnDeserialized()
    {
        JsonString = JsonSerializer.Serialize(this);
    }

    private Error ImportEntity(LdtkJson ldtkJson)
    {
        var entityCountMap = new System.Collections.Generic.Dictionary<string, int>();
        for (var i = 0; i < EntityInstances.Length; i++)
        {
            var instance = EntityInstances[i];
            var entityScenePath = ldtkJson.Defs
                .Entities
                .FirstOrDefault(definition => definition.Uid == instance.DefUid)!
                .EntityScenePath;

            var node2D = ResourceLoader.Load<PackedScene>(entityScenePath).Instantiate<Node2D>();

            node2D.Name = $"{instance.Identifier}-{entityCountMap.GetValueOrDefault(instance.Identifier).ToString()}";
            node2D.Position = new Vector2(instance.Px[0], instance.Px[1]);
            node2D.SetMeta("fields", Json.ParseString(JsonSerializer.Serialize(instance.FieldInstances)));

            Root.AddChild(node2D);

            var newCount = entityCountMap.TryGetValue(instance.Identifier, out var count) ? count + 1 : 1;
            entityCountMap[instance.Identifier] = newCount;
        }

        return Error.Ok;
    }

    private Error ImportTile(LdtkJson ldtkJson, Dictionary options)
    {
        var tileMap = (TileMap)Root;
        var layerDefinition = ldtkJson.Defs.Layers.FirstOrDefault(definition => definition.Uid == LayerDefUid);
        var tilesetDefinition = ldtkJson.Defs.Tilesets.FirstOrDefault(definition => definition.Uid == TilesetDefUid);

        tileMap.TileSet = tilesetDefinition!.TileSet;
        tileMap.SetLayerName(0, layerDefinition!.Identifier);
        tileMap.SetLayerModulate(0, new Color(1, 1, 1, (float)Opacity));
        tileMap.SetLayerEnabled(0, Visible);

        var tileInstances = Type == nameof(TypeEnum.Tiles) ? GridTiles : AutoLayerTiles;
        var sourceId = (int)tilesetDefinition.Uid;
        var source = (TileSetAtlasSource)tileMap.TileSet.GetSource(sourceId);
        for (var index = 0; index < tileInstances.Length; index++)
        {
            var tileInstance = tileInstances[index];
            var coords = new Vector2I((int)tileInstance.Px[0], (int)tileInstance.Px[1]) / (int)GridSize;
            var atlasCoords = tileInstance.T.AtlasCoords(source);
            tileMap.SetCell(0, coords, sourceId, atlasCoords);
            tileMap.GetCellTileData(0, coords).Modulate = new Color(1, 1, 1, (float)tileInstance.A);
        }

        return Type == nameof(TypeEnum.IntGrid) ? ImportIntGrid(ldtkJson, options) : Error.Ok;
    }

    private Error ImportIntGrid(LdtkJson ldtkJson, Dictionary options)
    {
        if (!options.GetValueOrDefault(LdtkImporterPlugin.OptionLevelImportIntGrid).AsBool())
        {
            GD.Print(
                $"   {LdtkImporterPlugin.OptionLevelImportIntGrid} is false, skip import IntGrid as child TileMap.");
            return Error.Ok;
        }

        var tileMap = new TileMap();
        tileMap.Name = Type;

        Root.AddChild(tileMap);

        var layerDefinition = ldtkJson.Defs.Layers.FirstOrDefault(definition => definition.Uid == LayerDefUid);
        var gridSize = (int)layerDefinition!.GridSize;
        var intGridValues = layerDefinition.IntGridValues;

        var tileSet = new TileSet();
        tileSet.TileSize = new Vector2I(gridSize, gridSize);
        tileSet.AddCustomDataLayer();
        tileSet.SetCustomDataLayerName(0, layerDefinition.Identifier);
        tileSet.SetCustomDataLayerType(0, Variant.Type.Dictionary);

        var image = Image.Create(gridSize * intGridValues.Length, gridSize, false, Image.Format.Rgb8);
        for (var index = 0; index < intGridValues.Length; index++)
        {
            var definition = intGridValues[index];
            var rect = new Rect2I(index * gridSize, 0, gridSize, gridSize);
            var color = Color.FromString(definition.Color, Colors.Magenta);

            image.FillRect(rect, color);
        }

        var imageTexture = ImageTexture.CreateFromImage(image);
        var source = new TileSetAtlasSource();

        source.Texture = imageTexture;
        source.TextureRegionSize = tileSet.TileSize;

        tileSet.AddSource(source, (int)layerDefinition.Uid);

        for (var index = 0; index < intGridValues.Length; index++)
        {
            var instance = intGridValues[index];
            var atlasCoords = new Vector2I(index, 0);

            source.CreateTile(atlasCoords);
            var tileData = source.GetTileData(atlasCoords, 0);
            tileData.SetCustomDataByLayerId(0, Json.ParseString(JsonSerializer.Serialize(instance)));
        }

        tileMap.TileSet = tileSet;
        tileMap.SetLayerName(0, layerDefinition.Identifier);
        tileMap.SetLayerModulate(0, new Color(1, 1, 1, (float)Opacity));
        tileMap.SetLayerEnabled(0, Visible);

        for (var index = 0; index < IntGridCsv.Length; index++)
        {
            //empty cell, skip.
            if (IntGridCsv[index] == 0) continue;

            var coords = new Vector2I(Mathf.FloorToInt(index % CWid), Mathf.FloorToInt(index / (float)CWid));
            var atlasCoords = (IntGridCsv[index] - 1).AtlasCoords(source);
            tileMap.SetCell(0, coords, (int)layerDefinition.Uid, atlasCoords);
        }

        return Error.Ok;
    }
}