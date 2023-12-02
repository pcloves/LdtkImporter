using System.Linq;
using Godot;
using Godot.Collections;

namespace LdtkImporter;

[GlobalClass]
[Tool]
[PostProcessor(ProcessorType.Entity)]
public partial class DefaultEntityPostProcessor : AbstractPostProcessor
{
    public override Node2D PostProcess(LdtkJson ldtkJson, Dictionary options, Node2D baseNode)
    {
        var prefix = options.GetValueOrDefault<string>(LdtkImporterPlugin.OptionGeneralPrefix);
        var instanceMeta = baseNode.GetMeta($"{prefix}_entityInstance").AsGodotDictionary();
        var definitionMeta = baseNode.GetMeta($"{prefix}_entityDefinition").AsGodotDictionary();
        var renderMode = (RenderMode)definitionMeta["renderMode"].AsInt32();

        if (renderMode == RenderMode.Tile)
        {
            var uid = instanceMeta["__tile"].AsGodotDictionary()["tilesetUid"].As<long>();
            var tilesetDefinition = ldtkJson.Defs.Tilesets.FirstOrDefault(definition => definition.Uid == uid);
            if (tilesetDefinition == null)
            {
                GD.PrintErr($"Get TilesetDefinition for entity:{instanceMeta["iid"].AsString()}, __tile.tilesetUid:{uid}");
                return baseNode;
            }

            if (tilesetDefinition.EmbedAtlas.HasValue)
            {
                GD.Print($"EmbedAtlas, ignore.");
                return baseNode;
            }
            
            //TODO:
        }


        return baseNode;
    }
}