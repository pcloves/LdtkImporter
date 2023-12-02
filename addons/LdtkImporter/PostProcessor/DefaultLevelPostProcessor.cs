using System.Linq;
using Godot;
using Godot.Collections;

namespace LdtkImporter;

[Tool]
[GlobalClass]
[PostProcessor(ProcessorType.Level)]
public partial class DefaultLevelPostProcessor : AbstractPostProcessor
{
    public override Node2D PostProcess(LdtkJson ldtkJson, Dictionary options, Node2D baseNode)
    {
        var prefix = options.GetValueOrDefault<string>(LdtkImporterPlugin.OptionGeneralPrefix);
        var instanceMeta = baseNode.GetMeta($"{prefix}_levelInstance");

        var iid = instanceMeta.AsGodotDictionary()["iid"].AsString();
        var level = ldtkJson.Levels.FirstOrDefault(level => level.Iid.Equals(iid), null);

        var bgColorNode = new ColorRect();
        bgColorNode.Name = $"{prefix}_{nameof(level.BgColor)}";
        bgColorNode.Color = Color.FromString(level.BgColor, Colors.Gray);
        bgColorNode.Size = new Vector2(level.PxWid, level.PxHei);

        baseNode.AddChild(bgColorNode);
        baseNode.MoveChild(bgColorNode, 0);
        bgColorNode.Owner = baseNode;

        if (string.IsNullOrEmpty(level.BgRelPath)) return baseNode;

        var bgPath = ldtkJson.Path.GetBaseDir().PathJoin(level.BgRelPath);
        var texture2D = ResourceLoader.Load<Texture2D>(bgPath);
        var bgImageNode = new Sprite2D();
        var bgPos = level.BgPos;

        bgImageNode.Name = $"{prefix}_{bgPath.GetBaseName().GetFile().Replace(" ", "-")}";
        bgImageNode.Texture = texture2D;
        bgImageNode.Centered = false;
        bgImageNode.RegionEnabled = true;
        bgImageNode.RegionRect = new Rect2((float)bgPos.CropRect[0], (float)bgPos.CropRect[1], (float)bgPos.CropRect[2], (float)bgPos.CropRect[3]);
        bgImageNode.Scale = new Vector2((float)bgPos.Scale[0], (float)bgPos.Scale[1]);
        bgImageNode.Offset = new Vector2(bgPos.TopLeftPx[0], bgPos.TopLeftPx[1]);

        baseNode.AddChild(bgImageNode);
        baseNode.MoveChild(bgImageNode, 1);
        bgImageNode.Owner = baseNode;


        return baseNode;
    }
}