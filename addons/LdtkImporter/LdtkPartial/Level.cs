using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using Godot.Collections;

namespace LdtkImporter;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public partial class Level : IImporter, IJsonOnDeserialized
{
    [JsonIgnore] public Node2D Root { get; set; }
    [JsonIgnore] public string ScenePath { get; set; }
    [JsonIgnore] public string JsonString { get; set; }
    [JsonIgnore] public int LevelIndex { get; set; }


    public Error PreImport(LdtkJson ldtkJson, string savePath, Dictionary options, Array<string> genFiles)
    {
        GD.Print($"  {Identifier}");

        var key = $"{LdtkImporterPlugin.OptionLevelScenes}/{Identifier}";
        var scenePath = options.GetValueOrDefault<string>(key);

        if (string.IsNullOrWhiteSpace(scenePath))
        {
            GD.Print($"   property:{key} is not set.");
            return Error.FileNotFound;
        }

        var prefix = options.GetValueOrDefault<string>(LdtkImporterPlugin.OptionGeneralPrefix);

        if (!ResourceLoader.Exists(scenePath))
        {
            GD.Print($"   level scene:{scenePath} is not exist, create it!");
            Root = new Node2D()
            {
                Name = $"{prefix}_{Identifier}",
            };
        }
        else
        {
            Root = ResourceLoader.Load<PackedScene>(scenePath).Instantiate<Node2D>();

            if (Root == null)
            {
                GD.Print($"  {scenePath} is not a packed scene resource.");
                return Error.Failed;
            }
        }

        ScenePath = scenePath;
        GD.Print($"   load level scene success:{scenePath}");

        Root.RemoveMetaByPrefix(prefix);
        Root.RemoveChildByNamePrefix(prefix);
        Root.Position = _getWorldPosition(ldtkJson);

        GD.Print("   PreImport Layer");
        foreach (var layerInstance in LayerInstances)
        {
            var error = layerInstance.PreImport(ldtkJson, savePath, options, genFiles);
            if (error != Error.Ok) return error;
        }

        return Error.Ok;
    }

    public Error Import(LdtkJson ldtkJson, string savePath, Dictionary options, Array<string> genFiles)
    {
        GD.Print($"  {Identifier}:{ScenePath}");

        var prefix = options.GetValueOrDefault<string>(LdtkImporterPlugin.OptionGeneralPrefix);
        var addLevelInstance2Meta = options.GetValueOrDefault<bool>(LdtkImporterPlugin.OptionLevelAddLevelInstanceToMeta);
        var fieldInstance = Json.ParseString(JsonSerializer.Serialize(FieldInstances));

        Root.SetMeta($"{prefix}_fieldInstances", fieldInstance);

        if (addLevelInstance2Meta)
        {
            Root.SetMeta($"{prefix}_levelInstance", Json.ParseString(JsonSerializer.Serialize(this)));
        }

        foreach (var layerInstance in LayerInstances)
        {
            GD.Print($"   Import Layer:{layerInstance.Identifier}:{layerInstance.Type}");
            var error = layerInstance.Import(ldtkJson, savePath, options, genFiles);
            if (error != Error.Ok) return error;
        }

        return Error.Ok;
    }

    private Vector2 _getWorldPosition(LdtkJson ldtkJson)
    {
        var position = Vector2.Zero;
        switch (ldtkJson.WorldLayout)
        {
            case WorldLayout.Free:
            case WorldLayout.GridVania:
                position = new Vector2(WorldX, WorldY);
                break;
            case WorldLayout.LinearHorizontal:
                var x = ldtkJson.Levels.Where(level => level.LevelIndex < LevelIndex).Select(level => level.PxWid).Sum();
                position = new Vector2(x, 0);
                break;
            case WorldLayout.LinearVertical:
                var y = ldtkJson.Levels
                    .Where(level => level.LevelIndex < LevelIndex)
                    .Select(level => level.PxHei)
                    .Sum();
                position = new Vector2(0, y);
                break;
        }


        return position;
    }

    private void _importBackgroundImage(Dictionary options, LdtkJson ldtkJson)
    {
        if (!string.IsNullOrEmpty(BgRelPath))
        {
            var prefix = options.GetValueOrDefault<string>(LdtkImporterPlugin.OptionGeneralPrefix);
            var bgNode = new Sprite2D();

            var bgPath = ldtkJson.Path.GetBaseDir().PathJoin(BgRelPath);
            var texture2D = ResourceLoader.Load<Texture2D>(bgPath);

            bgNode.Name = $"{prefix}_{bgPath.GetBaseName().GetFile().Replace(" ", "-")}";
            bgNode.Texture = texture2D;
            bgNode.Centered = false;
            bgNode.RegionEnabled = true;
            bgNode.RegionRect = new Rect2((float)BgPos.CropRect[0], (float)BgPos.CropRect[1], (float)BgPos.CropRect[2], (float)BgPos.CropRect[3]);
            bgNode.Scale = new Vector2((float)BgPos.Scale[0], (float)BgPos.Scale[1]);
            bgNode.Offset = new Vector2(BgPos.TopLeftPx[0], BgPos.TopLeftPx[1]);

            Root.AddChild(bgNode);
            bgNode.Owner = Root;
        }
    }

    public Error PostImport(LdtkJson ldtkJson, string savePath, Dictionary options, Array<string> genFiles)
    {
        GD.Print($"  save level scene:{ScenePath}");

        _importBackgroundImage(options, ldtkJson);

        foreach (var layerInstance in LayerInstances.Reverse())
        {
            Root.AddChild(layerInstance.Root);
            layerInstance.Root.SetOwnerRecursively(Root);
        }

        var packedScene = new PackedScene();
        packedScene.Pack(Root);

        DirAccess.MakeDirRecursiveAbsolute(ScenePath.GetBaseDir());
        ResourceSaver.Save(packedScene, ScenePath);

        genFiles.Add(ScenePath);
        return Error.Ok;
    }

    public void OnDeserialized()
    {
        JsonString = JsonSerializer.Serialize(this);
    }
}