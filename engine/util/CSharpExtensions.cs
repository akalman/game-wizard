using System.Collections.Generic;
using Godot;

namespace GameWizard.Engine.Util;

public static class CSharpExtensions
{
    public static bool IsEmpty<T>(this ICollection<T> collection)
    {
        return collection.Count == 0;
    }

    public static bool IsNullOrEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }

    public static U SafeGet<T, U>(this IDictionary<T, U> dict, T key)
    {
        if (!dict.TryGetValue(key, out var value))
            throw new GameWizardInternalException($"Could not find key {key} in map.");

        return value;
    }
}

