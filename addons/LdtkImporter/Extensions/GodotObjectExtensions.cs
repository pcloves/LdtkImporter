using System.Linq;
using Godot;

namespace LdtkImporter;

public static class GodotObjectExtensions
{
    public static void RemoveMetaByPrefix(this GodotObject godotObject, string prefix)
    {
        var metas = godotObject.GetMetaList()
            .Select(m => (string)m)
            .Where(meta => meta.StartsWith(prefix));
        foreach (var meta in metas)
        {
            godotObject.RemoveMeta(meta);
        }
    }
}