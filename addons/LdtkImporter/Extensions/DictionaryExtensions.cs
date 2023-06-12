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
}