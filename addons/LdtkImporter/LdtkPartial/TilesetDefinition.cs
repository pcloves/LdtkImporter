using System;
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

    [JsonIgnore]
    public System.Collections.Generic.Dictionary<long, HashSet<int>> LdtkTileInstanceId2GodotAlternativeId = new();

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

                if (!LdtkTileInstanceId2GodotAlternativeId.ContainsKey(customMetadata.TileId)) continue;

                foreach (var alternativeId in LdtkTileInstanceId2GodotAlternativeId[customMetadata.TileId])
                {
                    var tileData = source.GetTileData(atlasCoords, alternativeId);
                    if (tileData == null)
                    {
                        // TODO:
                        GD.PrintErr($"GetTileData failed, ldtk tileId:{customMetadata.TileId} atlasCoords:{atlasCoords}, alternativeId:{alternativeId}");
                        continue;
                    }
                    
                    var data = Json.ParseString(customMetadata.Data);
                    tileData.SetCustomDataByLayerId(customDataLayerIndex, data);

                    GD.Print($"   tileId:{customMetadata.TileId}/{atlasCoords}, data:{data}");
                }
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

    public long GetTileCx(long tileId)
    {
        return tileId - CWid * Convert.ToInt64(tileId / CWid);
    }

    public long GetTileCy(long tileId)
    {
        return Convert.ToInt64(tileId / CWid);
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
                var tileGridSize = (float)TileGridSize;
                var textureOrigin = new Vector2(tileInstance.Px[0] % tileGridSize, tileInstance.Px[1] % tileGridSize);
                var textureOriginPivot = textureOrigin / TileGridSize;

                tileInstance.AlternativeIdFlags = (AlternativeIdFlags)tileInstance.F;
                tileInstance.AlternativeIdFlags |= textureOriginPivot.X.PivotXFlags();
                tileInstance.AlternativeIdFlags |= textureOriginPivot.Y.PivotYFlags();

                var alternativeId = (int)tileInstance.AlternativeIdFlags;

                LdtkTileInstanceId2GodotAlternativeId.GetOrCreate(tileId).Add(alternativeId);

                if (alternativeId == 0) continue;

                tileInstance.TextureOrigin = new Vector2I(-(int)textureOrigin.X, -(int)textureOrigin.Y);

                if (source.HasAlternativeTile(atlasCoords, alternativeId)) continue;

                GD.Print(
                    $"   CreateAlternativeTile alternativeId:{alternativeId}/({tileInstance.AlternativeIdFlags}), LDTK tileId:{tileId}, atlasCoords:{atlasCoords}, TextureOrigin:{tileInstance.TextureOrigin}");
                var alternativeTile = source.CreateAlternativeTile(atlasCoords, alternativeId);
                if (alternativeTile == -1)
                {
                    //TODO: why?
                    GD.PrintErr("   CreateAlternativeTile failed");
                    continue;
                }

                var tileData = source.GetTileData(atlasCoords, alternativeId);
                if (tileData == null)
                {
                    GD.PrintErr($"   GetTileData failed, tileInstance:{tileInstance}");
                    continue;
                }

                var alternativeIdFlags = (AlternativeIdFlags)alternativeId;

                tileData.FlipH = alternativeIdFlags.HasFlag(AlternativeIdFlags.FlipH);
                tileData.FlipV = alternativeIdFlags.HasFlag(AlternativeIdFlags.FlipV);
                tileData.TextureOrigin = tileInstance.TextureOrigin;
            }
        }

        ResourceSaver.Save(TileSet, TileSet.ResourcePath);
    }
}