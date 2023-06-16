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
        var error = CheckOptionValid(options);
        if (error != Error.Ok) return error;

        GD.Print(" PreImport Tileset");
        foreach (var definition in Defs.Tilesets)
        {
            error = definition.PreImport(ldtkJson, savePath, options, genFiles);
            if (error != Error.Ok) return error;
        }

        GD.Print(" PreImport Entity");
        foreach (var definition in Defs.Entities)
        {
            error = definition.PreImport(ldtkJson, savePath, options, genFiles);
            if (error != Error.Ok) return error;
        }

        GD.Print(" PreImport Level");
        foreach (var level in Levels)
        {
            error = level.PreImport(ldtkJson, savePath, options, genFiles);
            if (error != Error.Ok) return error;
        }


        return Error.Ok;
    }

    private Error CheckOptionValid(Dictionary options)
    {
        var prefix2Add = options.GetValueOrDefault<string>(LdtkImporterPlugin.OptionGeneralPrefix2Add);
        var prefix2Remove = options.GetValueOrDefault<string>(LdtkImporterPlugin.OptionGeneralPrefix2Add);

        if (!prefix2Add.IsValidPrefix())
        {
            GD.Print($"invalid prefix:{prefix2Add}, must be empty or valid godot identifier.");
            return Error.Failed;
        }

        if (!prefix2Remove.IsValidPrefix())
        {
            GD.Print($"invalid prefix:{prefix2Remove}, must be empty or valid godot identifier.");
            return Error.Failed;
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

        return SaveLdkJson(savePath, options, genFiles);
    }

    private Error SaveLdkJson(string savePath, Dictionary options, Array<string> genFiles)
    {
        Node2D root;
        var worldScenePath = options.GetValueOrDefault<string>(LdtkImporterPlugin.OptionWorldWorldMapping);
        if (!ResourceLoader.Exists(worldScenePath))
        {
            GD.Print($" world scene:{worldScenePath} is not exist, create it!");
            root = new Node2D
            {
                Name = Path.GetBaseName().GetFile(),
            };
        }
        else
        {
            root = ResourceLoader.Load<PackedScene>(worldScenePath).Instantiate<Node2D>();
        }

        var prefix2Remove = options.GetValueOrDefault<string>(LdtkImporterPlugin.OptionGeneralPrefix2Remove);
        root.RemoveChildByNamePrefix(prefix2Remove);

        foreach (var level in Levels)
        {
            root.AddChild(level.Root);
            level.Root.Owner = root;
        }

        var packedScene = new PackedScene();
        packedScene.Pack(root);

        var path = $"{savePath}.{LdtkImporterPlugin.SaveExtension}";
        ResourceSaver.Save(packedScene, path);

        return Error.Ok;
    }
}