using System.Linq;
using Godot;
using Godot.Collections;
using Ldtk;

namespace LdtkImporter.addons.LdtkImporter;

[Tool]
public partial class LdtkWorldImporter : EditorImportPlugin
{
    public override string _GetImporterName() => "Ldtk Importer";

    public override string _GetVisibleName() => "LDTK World Scene";

    public override string[] _GetRecognizedExtensions() => new[] { "ldtk" };

    public override string _GetResourceType() => "Node2D";

    public override string _GetSaveExtension() => "tscn";

    public override float _GetPriority() => 0.1f;

    public override int _GetPresetCount() => 0;

    public override string _GetPresetName(int presetIndex) => "";

    public override Array<Dictionary> _GetImportOptions(string path, int presetIndex)
    {
        var options = new Array<Dictionary>
        {
            //world
            new()
            {
                { "name", "World" },
                { "default_value", true },
                { "usage", (int)PropertyUsageFlags.Group },
            },
            new()
            {
                { "name", "world_add_metadata" },
                { "default_value", false },
                { "hint_string", "If true, will add the original LDtk data as metadata." }
            },
            new()
            {
                { "name", "world_post_import_script" },
                { "default_value", "" },
                { "property_hint", (int)PropertyHint.File },
                { "hint_string", "*.cs;C# Script" }
            },

            //Tilesets
            new()
            {
                { "name", "Tilesets" },
                { "default_value", "" },
                { "usage", (int)PropertyUsageFlags.Group },
            },
            new()
            {
                { "name", "tileset_add_metadata" },
                { "default_value", true },
                { "hint_string", "If true, will add the original LDtk data as metadata." }
            },
            new()
            {
                { "name", "import_tileset_custom_data" },
                { "default_value", true },
                { "property_hint", (int)PropertyHint.File },
                { "hint_string", "*.tres;Resource File" }
            },
            new()
            {
                { "name", "tileset_post_import_script" },
                { "default_value", "" },
                { "property_hint", (int)PropertyHint.File },
                { "hint_string", "*.cs;C# Script" }
            },

            //Levels
            new()
            {
                { "name", "Levels" },
                { "default_value", "" },
                { "usage", (int)PropertyUsageFlags.Group },
            },
            new()
            {
                { "name", "level_add_metadata" },
                { "default_value", true },
                { "hint_string", "If true, will add the original LDtk data as metadata." }
            },
            new()
            {
                { "name", "pack_levels" },
                { "default_value", false },
                { "hint_string", "If true, will add the original LDtk data as metadata." }
            },
            new()
            {
                { "name", "level_post_import_script" },
                { "default_value", "" },
                { "property_hint", (int)PropertyHint.File },
                { "hint_string", "*.cs;C# Script" }
            },
        };

        var entityOptions = new Array<Dictionary>
        {
            //Entities
            new()
            {
                { "name", "Entities" },
                { "default_value", "" },
                { "usage", (int)PropertyUsageFlags.Group },
            },
            new()
            {
                { "name", "entity_add_metadata" },
                { "default_value", true },
                { "hint_string", "If true, will add the original LDtk data as metadata." }
            },
            new()
            {
                { "name", "Entity Scene" },
                { "default_value", "" },
                { "usage", (int)PropertyUsageFlags.Subgroup },
            },
        };


        var fileText = FileAccess.Open(path, FileAccess.ModeFlags.Read).GetAsText();
        var ldtkJson = LdtkJson.FromJson(fileText);
        var properties = ldtkJson.Defs.Entities
            .OrderBy(entityDefinition => entityDefinition.Identifier)
            .Select(entityDefinition => new Dictionary
                {
                    { "name", entityDefinition.Identifier },
                    { "default_value", "" },
                    { "property_hint", (int)PropertyHint.File },
                    { "hint_string", "*.tscn;godot scene" }
                }
            );

        entityOptions.AddRange(properties);

        options.AddRange(entityOptions);

        return options;
    }

    public override int _GetImportOrder() => 99;

    public override bool _GetOptionVisibility(string path, StringName optionName, Dictionary options) => true;

    public override Error _Import(string sourceFile, string savePath, Dictionary options,
        Array<string> platformVariants, Array<string> genFiles)
    {
        GD.Print($"path:{sourceFile}");
        
        var fileText = FileAccess.Open(sourceFile, FileAccess.ModeFlags.Read).GetAsText();
        var ldtkJson = LdtkJson.FromJson(fileText);
        
        
        return Error.Ok;
    }
}