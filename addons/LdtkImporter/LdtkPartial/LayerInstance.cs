using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using Godot.Collections;
using TileInstanceDictionary =
    System.Collections.Generic.Dictionary<long, System.Collections.Generic.List<LdtkImporter.TileInstance>>;

namespace LdtkImporter;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public partial class LayerInstance : IImporter, IJsonOnDeserialized
{
    [JsonIgnore] public string JsonString { get; set; }
    [JsonIgnore] public Node2D Root { get; set; }
    [JsonIgnore] public int MaxTileStackCount { get; set; } = 1;

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

        var prefix = options.GetValueOrDefault<string>(LdtkImporterPlugin.OptionGeneralPrefix);

        Root = func.Invoke();
        Root.Name = $"{prefix}_{Identifier}";
        Root.Position = new Vector2(PxTotalOffsetX, PxTotalOffsetY);
        Root.Visible = Visible;
        Root.Modulate = new Color(1, 1, 1, (float)Opacity);
        
        var tileMap = Root as TileMap;
        var tilesetDefinition = ldtkJson.Defs.Tilesets.FirstOrDefault(definition => definition.Uid == TilesetDefUid);
        if (tileMap != null)
        {
            tileMap.RemoveLayer(tileMap.GetLayersCount() - 1);
            tileMap.TileSet = tilesetDefinition?.TileSet;
        }

        CalculateTileStack();

        return Error.Ok;
    }
    
    public Error Import(LdtkJson ldtkJson, string savePath, Dictionary options, Array<string> genFiles)
    {
        var prefix = options.GetValueOrDefault<string>(LdtkImporterPlugin.OptionGeneralPrefix);
        var addLayerDefinition2Meta =
            options.GetValueOrDefault<bool>(LdtkImporterPlugin.OptionLevelAddLayerDefinitionToMeta);
        var addLayerInstance2Meta =
            options.GetValueOrDefault<bool>(LdtkImporterPlugin.OptionLevelAddLayerInstanceToMeta);
        var layerDefinition = ldtkJson.Defs.Layers.FirstOrDefault(definition => definition.Uid == LayerDefUid);
        
        if (addLayerDefinition2Meta)
        {
            Root.SetMeta($"{prefix}_layerDefinition", Json.ParseString(JsonSerializer.Serialize(layerDefinition)));
        }

        if (addLayerInstance2Meta)
        {
            Root.SetMeta($"{prefix}_layerInstance", Json.ParseString(JsonString));
        }
        
        Error error;
        switch (Type)
        {
            case nameof(TypeEnum.IntGrid):
            case nameof(TypeEnum.AutoLayer):
            case nameof(TypeEnum.Tiles):
                error = ImportTile(ldtkJson, options);
                break;
            case nameof(TypeEnum.Entities):
                error = ImportEntity(ldtkJson, options);
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

    private void CalculateTileStack()
    {
        var tileInstances = Type == nameof(TypeEnum.Tiles) ? GridTiles : AutoLayerTiles;
        MaxTileStackCount = tileInstances
            .GroupBy(ToCoords)
            .Select(grouping => grouping.Select((instance, i) => instance.Layer = i).Count())
            .DefaultIfEmpty(1)
            .Max();
    }

    private Error ImportEntity(LdtkJson ldtkJson, Dictionary options)
    {
        var prefix = options.GetValueOrDefault<string>(LdtkImporterPlugin.OptionGeneralPrefix);
        var addDefinition2Meta = options.GetValueOrDefault<bool>(LdtkImporterPlugin.OptionEntityAddDefinition2Meta);
        var addInstance2Meta = options.GetValueOrDefault<bool>(LdtkImporterPlugin.OptionEntityAddInstance2Meta);

        var entityCountMap = new System.Collections.Generic.Dictionary<string, int>();
        foreach (var entityInstance in EntityInstances)
        {
            var entityDefinition = ldtkJson.Defs
                .Entities
                .FirstOrDefault(definition => definition.Uid == entityInstance.DefUid)!;
            var entityScenePath = entityDefinition.EntityScenePath;

            var node2D = ResourceLoader.Load<PackedScene>(entityScenePath).Instantiate<Node2D>();

            node2D.Position = new Vector2(entityInstance.Px[0], entityInstance.Px[1]);
            node2D.Name = $"{node2D.Name}-{entityCountMap.GetValueOrDefault(entityInstance.Identifier).ToString()}";
            node2D.SetMeta($"{prefix}_fieldInstances",
                Json.ParseString(JsonSerializer.Serialize(entityInstance.FieldInstances)));

            if (addDefinition2Meta)
            {
                node2D.SetMeta($"{prefix}_entityDefinition",
                    Json.ParseString(entityDefinition.JsonString));
            }

            if (addInstance2Meta)
            {
                node2D.SetMeta($"{prefix}_entityInstance", Json.ParseString(JsonSerializer.Serialize(entityInstance)));
            }

            Root.AddChild(node2D);

            var newCount = entityCountMap.TryGetValue(entityInstance.Identifier, out var count) ? count + 1 : 1;
            entityCountMap[entityInstance.Identifier] = newCount;
        }

        return Error.Ok;
    }

    private Error ImportTile(LdtkJson ldtkJson, Dictionary options)
    {
        var tileMap = (TileMap)Root;
        var prefix = options.GetValueOrDefault<string>(LdtkImporterPlugin.OptionGeneralPrefix);
        var layerDefinition = ldtkJson.Defs.Layers.FirstOrDefault(definition => definition.Uid == LayerDefUid);
        var tilesetDefinition = ldtkJson.Defs.Tilesets.FirstOrDefault(definition => definition.Uid == TilesetDefUid);

        var layerNamePrefix = $"{prefix}_{layerDefinition!.Identifier}";
        for (var i = 0; i < MaxTileStackCount; i++)
        {
            tileMap.EnsureLayerExist($"{layerNamePrefix}_{i}");
        }

        tileMap.TileSet = tilesetDefinition?.TileSet;

        var tileInstances = Type == nameof(TypeEnum.Tiles) ? GridTiles : AutoLayerTiles;
        var sourceId = tilesetDefinition?.Uid ?? 0;
        var source = (TileSetAtlasSource)tileMap.TileSet?.GetSource((int)sourceId);
        foreach (var tileInstance in tileInstances)
        {
            var coords = ToCoords(tileInstance);
            var atlasCoords = tileInstance.T.AtlasCoords(source);
            var alternativeId = (int)tileInstance.AlternativeIdFlags;

            tileMap.SetCell(tileInstance.Layer, coords, (int)sourceId, atlasCoords, alternativeId);
            var cellTileData = tileMap.GetCellTileData(tileInstance.Layer, coords, alternativeId != 0);
            if (cellTileData == null)
            {
                GD.PrintErr(
                    $"   GetCellTileData failed, layer:{tileInstance.Layer}, coords:{coords}, atlasCoords:{atlasCoords}, alternativeId:{alternativeId}/({tileInstance.AlternativeIdFlags})");
                continue;
            }

            cellTileData.Modulate = new Color(1, 1, 1, (float)tileInstance.A);
        }

        return Type == nameof(TypeEnum.IntGrid) ? ImportIntGrid(ldtkJson, options) : Error.Ok;
    }

    private Error ImportIntGrid(LdtkJson ldtkJson, Dictionary options)
    {
        if (!options.GetValueOrDefault<bool>(LdtkImporterPlugin.OptionLevelImportIntGrid))
        {
            GD.Print(
                $"   {LdtkImporterPlugin.OptionLevelImportIntGrid} is false, skip import IntGrid as child TileMap.");
            return Error.Ok;
        }

        var prefix = options.GetValueOrDefault<string>(LdtkImporterPlugin.OptionGeneralPrefix);
        var tileMap = new TileMap();
        tileMap.Name = $"{prefix}_{Type}";

        Root.AddChild(tileMap);

        var layerDefinition = ldtkJson.Defs.Layers.FirstOrDefault(definition => definition.Uid == LayerDefUid);
        var gridSize = (int)layerDefinition!.GridSize;
        var intGridValues = layerDefinition.IntGridValues;

        var tileSet = new TileSet();
        tileSet.TileSize = new Vector2I(gridSize, gridSize);
        tileSet.AddCustomDataLayerIfNotExist($"{prefix}_{layerDefinition.Identifier}", Variant.Type.Dictionary);

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

        var layer = tileMap.EnsureLayerExist($"{prefix}_{layerDefinition!.Identifier}");
        tileMap.TileSet = tileSet;
        tileMap.SetLayerModulate(layer, new Color(1, 1, 1, (float)Opacity));
        tileMap.SetLayerEnabled(layer, Visible);

        for (var index = 0; index < IntGridCsv.Length; index++)
        {
            //empty cell, skip.
            if (IntGridCsv[index] == 0) continue;

            var coords = new Vector2I(Mathf.FloorToInt(index % CWid), Mathf.FloorToInt(index / (float)CWid));
            var atlasCoords = (IntGridCsv[index] - 1).AtlasCoords(source);
            tileMap.SetCell(layer, coords, (int)layerDefinition.Uid, atlasCoords);
        }

        return Error.Ok;
    }
    
    private Vector2I ToCoords(TileInstance tileInstance)
    {
        var coords = new Vector2I((int)tileInstance.Px[0], (int)tileInstance.Px[1]) / (int)(GridSize);

        return coords;
    }
}