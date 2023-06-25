﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
        TileSet.AddCustomDataLayerIfNotExist($"{prefix2Add}_{Identifier}", Variant.Type.Dictionary);
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
        var customLayerName = $"{prefix2Add}_{Identifier}";
        var sourceId = TileSet.GetSourceIdByName(customLayerName);
        if (sourceId == -1)
        {
            GD.Print($"   none TileSetAtlasSource named:{customLayerName} in {tileSetPath}, named it, and try again.");
            return Error.Failed;
        }

        var customDataLayerIndex = TileSet.GetCustomDataLayerByName(customLayerName);

        if (options.GetValueOrDefault<bool>(LdtkImporterPlugin.OptionTilesetAddMeta))
        {
            var meta = Json.ParseString(JsonString);
            TileSet.SetMeta($"{prefix2Add}_tilesets", meta);
        }

        var source = (TileSetAtlasSource)TileSet.GetSource(sourceId);

        UpdateTile(ldtkJson, source);

        var importTileCustomData =
            options.GetValueOrDefault<bool>(LdtkImporterPlugin.OptionTilesetImportTileCustomData);
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

        source.ResourceName = $"{prefix2Add}_{Identifier}";
        source.Margins = new Vector2I((int)Padding, (int)Padding);
        source.Separation = new Vector2I((int)Spacing, (int)Spacing);
        source.Texture = texture2D;
        source.TextureRegionSize = new Vector2I((int)TileGridSize, (int)TileGridSize);


        DirAccess.MakeDirRecursiveAbsolute(tileSetPath.GetBaseDir());

        ResourceSaver.Save(tileSet, tileSetPath);
    }

    private void UpdateTile(LdtkJson ldtkJson, TileSetAtlasSource source)
    {
        //create single tile
        var tileId2FlipBitsMap = ldtkJson.Levels
            .AsParallel()
            .SelectMany(level => level.LayerInstances)
            .Where(instance => instance.TilesetDefUid == Uid)
            .SelectMany(instance => instance.GridTiles.Concat(instance.AutoLayerTiles))
            .Select(instance => new KeyValuePair<long, TileInstance>(instance.T, instance))
            .GroupBy(pair => pair.Key)
            .ToDictionary(pairs => pairs.Key,
                pairs => pairs.Select(pair => pair.Value).ToHashSet());

        foreach (var tileId in tileId2FlipBitsMap.Keys)
        {
            var atlasCoords = tileId.AtlasCoords(source);

            if (!source.HasTile(atlasCoords))
            {
                source.CreateTile(atlasCoords);
                GD.Print($"   CreateTile, atlasCoords:{atlasCoords}, size:{Vector2I.One}");
            }

            foreach (var tileInstance in tileId2FlipBitsMap[tileId])
            {
                tileInstance.AlternativeIdFlags = (AlternativeIdFlags)tileInstance.F;

                var textureOriginX = -(int)(tileInstance.Px[0] % TileGridSize);
                if (Math.Abs(Math.Abs(textureOriginX / (float)TileGridSize) - 0.5f) < 0.01f)
                    tileInstance.AlternativeIdFlags |= AlternativeIdFlags.PivotXHalf;

                var textureOriginY = -(int)(tileInstance.Px[1] % TileGridSize);
                if (Math.Abs(Math.Abs(textureOriginY / (float)TileGridSize ) - 0.5f) < 0.01f)
                    tileInstance.AlternativeIdFlags |= AlternativeIdFlags.PivotYHalf;

                var alternativeId = (int)tileInstance.AlternativeIdFlags;
                if (alternativeId == 0) continue;

                tileInstance.TextureOrigin = new Vector2I(textureOriginX, textureOriginY);

                if (source.HasAlternativeTile(atlasCoords, alternativeId)) continue;

                source.CreateAlternativeTile(atlasCoords, alternativeId);
                var tileData = source.GetTileData(atlasCoords, alternativeId);

                var alternativeIdFlags = (AlternativeIdFlags)alternativeId;

                tileData.FlipH = alternativeIdFlags.HasFlag(AlternativeIdFlags.FlipH);
                tileData.FlipV = alternativeIdFlags.HasFlag(AlternativeIdFlags.FlipV);
                tileData.TextureOrigin = tileInstance.TextureOrigin;
                GD.Print(
                    $"   CreateAlternativeTile alternativeId:{alternativeId}({alternativeIdFlags}), LDTK tileId:{tileId}, atlasCoords:{atlasCoords}, TextureOrigin:{tileInstance.TextureOrigin}");
            }
        }
    }
}

[Flags]
public enum AlternativeIdFlags
{
    None = 0,
    FlipH = 1, //X flip only
    FlipV = 2, //Y flip only

    PivotXHalf = 4, //pivotX: 0.5
    PivotYHalf = 8, //pivotY: 0.5
}