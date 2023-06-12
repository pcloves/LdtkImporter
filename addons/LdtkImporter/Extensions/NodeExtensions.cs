using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Godot;

namespace LdtkImporter;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class NodeExtensions
{
    public static void RemoveChildPrefix(this Node node, string childNamePrefix)
    {
        var children = node.GetChildren().Where(child => child.Name.ToString().StartsWith(childNamePrefix));
        foreach (var child in children)
        {
            node.RemoveChild(child);
            child.QueueFree();
        }
    }

    public static T GetChild<T>(this Node node, string childName) where T : Node
    {
        return node.GetChildren().FirstOrDefault(node1 => node1.Name == childName) as T;
    }

    public static void SetOwnerRecursively(this Node node, Node owner)
    {
        node.Owner = owner;
        foreach (var child in node.GetChildren())
        {
            child.SetOwnerRecursively(owner);
        }
    }
}