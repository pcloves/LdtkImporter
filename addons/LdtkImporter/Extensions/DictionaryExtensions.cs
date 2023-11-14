using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace LdtkImporter;

public static class DictionaryExtensions
{
    public static T GetValueOrDefault<[MustBeVariant] T>(this Dictionary dictionary, Variant key)
    {
        return dictionary.GetValueOrDefault(key, default!).As<T>();
    }

    public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TValue : new()
    {
        if (dict.TryGetValue(key, out var val))
            return val;

        val = new TValue();
        dict.Add(key, val);

        return val;
    }
}