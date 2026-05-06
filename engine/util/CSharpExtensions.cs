using System.Collections.Generic;
using Godot;

namespace GameWizard.Engine.Util;

public static class CSharpExtensions
{
    public static bool IsEmpty<T>(this ICollection<T> collection)
    {
        return collection.Count == 0;
    }
}

