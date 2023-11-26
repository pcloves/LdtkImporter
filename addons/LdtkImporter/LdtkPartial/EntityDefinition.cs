using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using Godot.Collections;

namespace LdtkImporter;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public partial class EntityDefinition : IImporter, IJsonOnDeserialized
{
    [JsonIgnore] public Node Root { get; set; }
    [JsonIgnore] public string EntityScenePath { get; set; }
    [JsonIgnore] public string JsonString { get; set; }

    public Error PreImport(LdtkJson ldtkJson, string savePath, Dictionary options, Array<string> genFiles)
    {
        GD.Print($"  {Identifier}");

        var key = $"{LdtkImporterPlugin.OptionEntityScenes}/{Identifier}";
        var scenePath = options.GetValueOrDefault<string>(key);
        var prefix = options.GetValueOrDefault<string>(LdtkImporterPlugin.OptionGeneralPrefix);

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
                Name = $"{prefix}_{Identifier}",
            };

            var packedScene = new PackedScene();
            packedScene.Pack(Root);

            DirAccess.MakeDirRecursiveAbsolute(scenePath.GetBaseDir());
            ResourceSaver.Save(packedScene, scenePath);
        }

        Root = ResourceLoader.Load<PackedScene>(scenePath).Instantiate();

        if (Root == null)
        {
            GD.Print($"  {scenePath} is not a packed scene resource.");
            return Error.Failed;
        }

        Root.RemoveChildByNamePrefix(prefix);
        Root.RemoveMetaByPrefix(prefix);

        EntityScenePath = scenePath;
        GD.Print($"   load entity scene success:{scenePath}");

        return Error.Ok;
    }

    public Error Import(LdtkJson ldtkJson, string savePath, Dictionary options, Array<string> genFiles)
    {
        GD.Print($"  {Identifier}:{EntityScenePath}");

        var prefix = options.GetValueOrDefault<string>(LdtkImporterPlugin.OptionGeneralPrefix);
        var addDefinition2Meta = options.GetValueOrDefault<bool>(LdtkImporterPlugin.OptionEntityAddDefinition2Meta);
        var definitionMeta = Json.ParseString(JsonString);

        if (addDefinition2Meta)
        {
            Root.SetMeta($"{prefix}_entityDefinition", definitionMeta);
        }
        
        return Error.Ok;
    }

    public Error PostImport(LdtkJson ldtkJson, string savePath, Dictionary options, Array<string> genFiles)
    {
        GD.Print($"  save entity scene:{EntityScenePath}");

        var packedScene = new PackedScene();
        packedScene.Pack(Root);

        ResourceSaver.Save(packedScene, EntityScenePath);

        genFiles.Add(EntityScenePath);

        return Error.Ok;
    }

    public void OnDeserialized()
    {
        JsonString = JsonSerializer.Serialize(this);
    }
}