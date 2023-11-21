using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Godot;
using Godot.Collections;
using Environment = System.Environment;

namespace LdtkImporter;

[Tool]
[SuppressMessage("ReSharper", "UnusedParameter.Local")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
[SuppressMessage("Performance", "CA1822:将成员标记为 static")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public partial class LdtkImporterPlugin : EditorImportPlugin
{
    public const string OptionGeneral = "General";
    public const string OptionGeneralPrefix2Remove = $"{OptionGeneral}/prefix_remove";
    public const string OptionGeneralPrefix2Add = $"{OptionGeneral}/prefix_add";

    public const string OptionWorld = "World";
    public const string OptionWorldWorldMapping = $"{OptionWorld}/WorldMapping";

    public const string OptionTileset = "Tileset";
    public const string OptionTilesetAddMeta = $"{OptionTileset}/add_metadata";
    public const string OptionTilesetCreate = $"{OptionTileset}/create_if_not_exists";
    public const string OptionTilesetMapping = $"{OptionTileset}/Mapping";
    public const string OptionTilesetImportTileCustomData = $"{OptionTileset}/import_LDTK_tile_custom_data";

    public const string OptionLevel = "Level";
    public const string OptionLevelMapping = $"{OptionLevel}/Mapping";
    public const string OptionLevelImportIntGrid = $"{OptionLevel}/import_IntGrid";

    public const string OptionEntity = "Entity";
    public const string OptionEntityMapping = $"{OptionEntity}/Mapping";

    public const string SaveExtension = "tscn";

    public override string _GetImporterName() => "Ldtk Importer";
    public override string _GetVisibleName() => "LDTK World Scene";
    public override string[] _GetRecognizedExtensions() => new[] { "ldtk" };
    public override string _GetResourceType() => "Node2D";
    public override string _GetSaveExtension() => SaveExtension;
    public override float _GetPriority() => 0.1f;
    public override int _GetPresetCount() => 0;
    public override string _GetPresetName(int presetIndex) => "";
    public override int _GetImportOrder() => 99;
    public override bool _GetOptionVisibility(string path, StringName optionName, Dictionary options) => true;

    private static readonly object _importLock = new();
    private LdtkJson _ldtkJson;
    private string _ldtkFileName;

    public override Array<Dictionary> _GetImportOptions(string path, int presetIndex)
    {
        _ldtkFileName = path.GetBaseName().GetFile();
        _ldtkJson = LdtkJson.FromPath(path);

        var options = new Array<Dictionary>();

        options.AddRange(GeneralOptions(path, presetIndex));
        options.AddRange(WorldOptions(path, presetIndex));
        options.AddRange(TilesetOptions(path, presetIndex));
        options.AddRange(EntityOptions(path, presetIndex));
        options.AddRange(LevelOptions(path, presetIndex));

        return options;
    }

    private IEnumerable<Dictionary> GeneralOptions(string path, int presetIndex)
    {
        return new Array<Dictionary>
        {
            new()
            {
                { "name", OptionGeneralPrefix2Remove },
                { "default_value", "LDTK" },
                { "hint_string", "when importing, all scene node and meta key with this prefix will be remove." }
            },
            new()
            {
                { "name", OptionGeneralPrefix2Add },
                { "default_value", "LDTK" },
                { "hint_string", "the importing scene node name and meta name will be prefixed." }
            },
        };
    }

    private IEnumerable<Dictionary> WorldOptions(string path, int presetIndex)
    {
        return new Array<Dictionary>
        {
            new()
            {
                { "name", OptionWorldWorldMapping },
                {
                    "default_value",
                    $"{path.GetBaseDir().PathJoin(_ldtkFileName).PathJoin(_ldtkFileName)}.{_GetSaveExtension()}"
                },
                { "property_hint", (int)PropertyHint.File },
                { "hint_string", "*.tscn;Godot Scene" }
            },
        };
    }

    private IEnumerable<Dictionary> TilesetOptions(string path, int presetIndex)
    {
        var tilesetBaseDir = path.GetBaseDir().PathJoin($"{_ldtkFileName}/Tileset");
        var options = new Array<Dictionary>
        {
            new()
            {
                { "name", OptionTilesetCreate },
                { "default_value", true },
                { "hint_string", "If true, will create tileset automatically if not exist." }
            },
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

        var tilesetMapping = _ldtkJson.Defs.Tilesets
            .OrderBy(definition => definition.Identifier)
            .Select(definition => new Dictionary
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
        var levelBaseDir = path.GetBaseDir().PathJoin($"{_ldtkFileName}/Level");
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
        var entityBaseDir = path.GetBaseDir().PathJoin($"{_ldtkFileName}/Entity");
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
        lock (_importLock)
        {
            GD.Print($"Import begin, LDTK file path:{sourceFile}, object:{_importLock.GetHashCode()}  thread:{Environment.CurrentManagedThreadId}, godot main id:{OS.GetMainThreadId()}");

            var error = _ldtkJson.PreImport(_ldtkJson, savePath, options, genFiles);
            if (error != Error.Ok)
            {
                GD.PrintErr($" PreImport failed, error:{error}");
                return error;
            }

            error = _ldtkJson.Import(_ldtkJson, savePath, options, genFiles);
            if (error != Error.Ok)
            {
                GD.PrintErr($" PreImport failed, error:{error}");
                return error;
            }

            error = _ldtkJson.PostImport(_ldtkJson, savePath, options, genFiles);
            if (error != Error.Ok)
            {
                GD.PrintErr($" PreImport failed, error:{error}");
                return error;
            }

            var worldScenePath = options.GetValueOrDefault<string>(OptionWorldWorldMapping);
            DirAccess.CopyAbsolute($"{savePath}.{SaveExtension}", worldScenePath);

            genFiles.Add(worldScenePath);

            GD.Print($"Import success thread:{Environment.CurrentManagedThreadId}");

            return Error.Ok;
        }
    }
}