using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Godot;
using Godot.Collections;

namespace LdtkImporter;

[Tool]
[SuppressMessage("ReSharper", "UnusedParameter.Local")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
[SuppressMessage("Performance", "CA1822:将成员标记为 static")]
public partial class LdtkImporterPlugin : EditorImportPlugin
{
    public const string OptionWorld = "World";

    public const string OptionTileset = "Tileset";
    public const string OptionTilesetAddMeta = $"{OptionTileset}/add_metadata";
    public const string OptionTilesetMapping = $"{OptionTileset}/Mapping";
    public const string OptionTilesetImportTileCustomData = $"{OptionTileset}/import_LDTK_tile_custom_data";

    public const string OptionLevel = "Level";
    public const string OptionLevelMapping = $"{OptionLevel}/Mapping";
    public const string OptionLevelImportIntGrid = $"{OptionLevel}/import_IntGrid";

    public const string OptionEntity = "Entity";
    public const string OptionEntityMapping = $"{OptionEntity}/Mapping";

    public override string _GetImporterName() => "Ldtk Importer";
    public override string _GetVisibleName() => "LDTK World Scene";
    public override string[] _GetRecognizedExtensions() => new[] { "ldtk" };
    public override string _GetResourceType() => "Node2D";
    public override string _GetSaveExtension() => "tscn";
    public override float _GetPriority() => 0.1f;
    public override int _GetPresetCount() => 0;
    public override string _GetPresetName(int presetIndex) => "";
    public override int _GetImportOrder() => 99;
    public override bool _GetOptionVisibility(string path, StringName optionName, Dictionary options) => true;

    private LdtkJson _ldtkJson;

    public override Array<Dictionary> _GetImportOptions(string path, int presetIndex)
    {
        _ldtkJson = LdtkJson.FromPath(path);

        var options = new Array<Dictionary>();

        options.AddRange(WorldOptions(path, presetIndex));
        options.AddRange(TilesetOptions(path, presetIndex));
        options.AddRange(EntityOptions(path, presetIndex));
        options.AddRange(LevelOptions(path, presetIndex));

        return options;
    }

    private IEnumerable<Dictionary> WorldOptions(string path, int presetIndex)
    {
        return new Array<Dictionary>
        {
            new()
            {
                { "name", $"{OptionWorld}/add_metadata" },
                { "default_value", false },
                { "hint_string", "If true, will add the original LDtk data as metadata." }
            },
        };
    }

    private IEnumerable<Dictionary> TilesetOptions(string path, int presetIndex)
    {
        var tilesetBaseDir = path.GetBaseDir().PathJoin("Tileset");
        var options = new Array<Dictionary>
        {
            new()
            {
                { "name", OptionTilesetAddMeta },
                { "default_value", true },
                { "hint_string", "If true, will add the original LDtk data as metadata with key:ldtk." }
            },
            new()
            {
                { "name", OptionTilesetImportTileCustomData },
                { "default_value", true },
                {
                    "hint_string",
                    "If true, will add custom layer data for each godot tile using Ldtk tile custom data."
                }
            },
        };

        var tilesetMapping = _ldtkJson.Defs.Tilesets.OrderBy(definition => definition.Identifier)
            .Select(definition => new Dictionary()
                {
                    { "name", $"{OptionTilesetMapping}/{definition.Identifier}" },
                    { "default_value", $"{tilesetBaseDir.PathJoin(definition.Identifier)}.tres" },
                    { "property_hint", (int)PropertyHint.File },
                    { "hint_string", "*.tres;Tileset Resource" }
                }
            );

        options.AddRange(tilesetMapping);

        return options;
    }

    private IEnumerable<Dictionary> LevelOptions(string path, int presetIndex)
    {
        var levelBaseDir = path.GetBaseDir().PathJoin("Level");
        var options = new Array<Dictionary>
        {
            new()
            {
                { "name", $"{OptionLevel}/level_add_metadata" },
                { "default_value", true },
                { "hint_string", "If true, will add the original LDtk data as metadata." }
            },
            new()
            {
                { "name", OptionLevelImportIntGrid },
                { "default_value", false },
                { "hint_string", "If true, will import IntGrid as a child TileMap node." }
            },
        };

        var levelMapping = _ldtkJson.Levels
            .OrderBy(definition => definition.Identifier)
            .Select(definition => new Dictionary()
                {
                    { "name", $"{OptionLevelMapping}/{definition.Identifier}" },
                    { "default_value", $"{levelBaseDir.PathJoin(definition.Identifier)}.tscn" },
                    { "property_hint", (int)PropertyHint.File },
                    { "hint_string", "*.tscn;Godot Scene" }
                }
            );

        options.AddRange(levelMapping);

        return options;
    }

    private IEnumerable<Dictionary> EntityOptions(string path, int presetIndex)
    {
        var entityBaseDir = path.GetBaseDir().PathJoin("Entity");
        var options = new Array<Dictionary>
        {
            new()
            {
                { "name", $"{OptionEntity}/entity_add_metadata" },
                { "default_value", true },
                { "hint_string", "If true, will add the original LDtk data as metadata." }
            },
        };

        var properties = _ldtkJson.Defs.Entities
            .OrderBy(definition => definition.Identifier)
            .Select(definition => new Dictionary
                {
                    { "name", $"{OptionEntityMapping}/{definition.Identifier}" },
                    { "default_value", $"{entityBaseDir.PathJoin(definition.Identifier)}.tscn" },
                    { "property_hint", (int)PropertyHint.File },
                    { "hint_string", "*.tscn;Godot Scene" }
                }
            );

        options.AddRange(properties);

        return options;
    }

    public override Error _Import(string sourceFile, string savePath, Dictionary options,
        Array<string> platformVariants, Array<string> genFiles)
    {
        GD.Print($"Import begin, LDTK file path:{sourceFile}");

        var error = _ldtkJson.PreImport(_ldtkJson, savePath, options, genFiles);
        if (error != Error.Ok) return error;

        error = _ldtkJson.Import(_ldtkJson, savePath, options, genFiles);
        if (error != Error.Ok) return error;

        error = _ldtkJson.PostImport(_ldtkJson, savePath, options, genFiles);
        if (error != Error.Ok) return error;

        var packedScene = new PackedScene();
        var node2D = new Node2D();

        packedScene.Pack(node2D);

        ResourceSaver.Save(packedScene, $"{savePath}.{_GetSaveExtension()}");

        GD.Print($"Import success");

        return Error.Ok;
    }
}