using System;
using System.Collections.Generic;
using System.Linq;

namespace GameWizard.Engine.Util;

public static class GWEnumerableExtensions
{
    public static IEnumerable<T> WhereNot<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
    {
        return collection.Where(Not(predicate));
    }

    public static Func<T, bool> Not<T>(this Func<T, bool> func)
    {
        return arg => !func(arg);
    }
}