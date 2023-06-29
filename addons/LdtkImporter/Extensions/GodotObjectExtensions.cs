using System.Linq;
using Godot;

namespace LdtkImporter;

public static class GodotObjectExtensions
{
    public static void RemoveMetaPrefix(this GodotObject godotObject, string metaPrefix)
    {
        var metas = godotObject.GetMetaList()
            .Select(m => (string)m)
            .Where(meta => meta.StartsWith(metaPrefix));
        foreach (var meta in metas)
        {
            godotObject.RemoveMeta(meta);
        }
    }
}