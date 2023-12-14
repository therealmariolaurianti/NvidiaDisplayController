using System;
using System.Collections.Generic;

namespace NvidiaDisplayController.Global.Extensions;

public static class LinqExtensions
{
    public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
    {
        foreach (var item in items)
            action(item);
    }
}