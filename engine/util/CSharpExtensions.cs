using System.Collections.Generic;

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

    public static TB SafeGet<TA, TB>(this IDictionary<TA, TB> dict, TA key)
    {
        if (!dict.TryGetValue(key, out var value))
            throw new GameWizardInternalException($"Could not find key {key} in map.");

        return value;
    }
}

