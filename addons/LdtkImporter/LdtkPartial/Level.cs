using System.Collections.Generic;
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

    public Error PreImport(LdtkJson ldtkJson, string savePath, Dictionary options, Array<string> genFiles)
    {
        GD.Print($"  {Identifier}");

        var key = $"{LdtkImporterPlugin.OptionLevelMapping}/{Identifier}";
        var scenePath = options.GetValueOrDefault(key).AsString().Trim();

        if (string.IsNullOrWhiteSpace(scenePath))
        {
            GD.Print($"   property:{key} is not set.");
            return Error.FileNotFound;
        }

        if (!ResourceLoader.Exists(scenePath))
        {
            GD.Print($"   entity scene:{scenePath} is not exist, create it!");
            Root = new Node2D()
            {
                Name = Identifier,
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

        var level = Json.ParseString(JsonString);
        var fieldInstance = Json.ParseString(JsonSerializer.Serialize(FieldInstances));

        Root.SetMeta("level", level);
        Root.SetMeta("fieldInstances", fieldInstance);

        Root.Position = new Vector2(WorldX, WorldY);
        // GD.Print($"   set meta:ldtk -> {meta}");

        foreach (var layerInstance in LayerInstances)
        {
            GD.Print($"   Import Layer:{layerInstance.Identifier}:{layerInstance.Type}");
            var error = layerInstance.Import(ldtkJson, savePath, options, genFiles);
            if (error != Error.Ok) return error;
        }

        return Error.Ok;
    }

    public Error PostImport(LdtkJson ldtkJson, string savePath, Dictionary options, Array<string> genFiles)
    {
        GD.Print($"  save entity scene:{ScenePath}");

        foreach (var layerInstance in LayerInstances.Reverse())
        {
            Root.RemoveChild(layerInstance.Root.Name);
            Root.AddChild(layerInstance.Root);
            layerInstance.Root.SetOwnerRecursively(Root);
        }

        var packedScene = new PackedScene();
        packedScene.Pack(Root);

        ResourceSaver.Save(packedScene, ScenePath);

        genFiles.Add(ScenePath);
        return Error.Ok;
    }

    public void OnDeserialized()
    {
        JsonString = JsonSerializer.Serialize(this);
    }
}