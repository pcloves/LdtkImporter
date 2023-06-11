using Godot;
using Godot.Collections;

namespace LdtkImporter;

public partial class LdtkJson : IImporter
{
    public string Path { get; set; }

    public static LdtkJson FromPath(string path)
    {
        var json = FileAccess.Open(path, FileAccess.ModeFlags.Read).GetAsText();
        var ldtkJson = FromJson(json);

        ldtkJson.Path = path;

        return ldtkJson;
    }

    public Error PreImport(LdtkJson ldtkJson, string savePath, Dictionary options, Array<string> genFiles)
    {
        GD.Print(" PreImport Tileset");
        foreach (var definition in Defs.Tilesets)
        {
            var error = definition.PreImport(ldtkJson, savePath, options, genFiles);
            if (error != Error.Ok) return error;
        }

        GD.Print(" PreImport Entity");
        foreach (var definition in Defs.Entities)
        {
            var error = definition.PreImport(ldtkJson, savePath, options, genFiles);
            if (error != Error.Ok) return error;
        }

        GD.Print(" PreImport Level");
        foreach (var level in Levels)
        {
            var error = level.PreImport(ldtkJson, savePath, options, genFiles);
            if (error != Error.Ok) return error;
        }

        return Error.Ok;
    }

    public Error Import(LdtkJson ldtkJson, string savePath, Dictionary options, Array<string> genFiles)
    {
        GD.Print(" Import Tileset");
        foreach (var definition in Defs.Tilesets)
        {
            var error = definition.Import(ldtkJson, savePath, options, genFiles);
            if (error != Error.Ok) return error;
        }

        GD.Print(" Import Entity");
        foreach (var definition in Defs.Entities)
        {
            var error = definition.Import(ldtkJson, savePath, options, genFiles);
            if (error != Error.Ok) return error;
        }

        GD.Print(" Import Level");
        foreach (var level in Levels)
        {
            var error = level.Import(ldtkJson, savePath, options, genFiles);
            if (error != Error.Ok) return error;
        }

        return Error.Ok;
    }

    public Error PostImport(LdtkJson ldtkJson, string savePath, Dictionary options, Array<string> genFiles)
    {
        GD.Print(" PostImport Tileset");
        foreach (var definition in Defs.Tilesets)
        {
            var error = definition.PostImport(ldtkJson, savePath, options, genFiles);
            if (error != Error.Ok) return error;
        }

        GD.Print(" PostImport Entity");
        foreach (var definition in Defs.Entities)
        {
            var error = definition.PostImport(ldtkJson, savePath, options, genFiles);
            if (error != Error.Ok) return error;
        }

        GD.Print(" PostImport Level");
        foreach (var level in Levels)
        {
            var error = level.PostImport(ldtkJson, savePath, options, genFiles);
            if (error != Error.Ok) return error;
        }

        return Error.Ok;
    }
}