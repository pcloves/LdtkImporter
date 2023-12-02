using Godot;
using Godot.Collections;

namespace LdtkImporter;

[Tool]
[GlobalClass]
[PostProcessor(ProcessorType.World)]
public partial class DefaultWorldPostProcessor : AbstractPostProcessor
{
    public override Node2D PostProcess(LdtkJson ldtkJson, Dictionary options, Node2D baseNode)
    {
        return baseNode;
    }
}