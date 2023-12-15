namespace AdventOfCode.Helpers;
using System.Collections.Generic;

public static class IListHelper
{
    /// <summary>Removes the first instance in <paramref name="list"/> that matches <paramref name="predicate"/>.</summary>
    /// <returns>True if an item was removed, false otherwise.</returns>
    public static bool RemoveFirst<T>(this IList<T> list, Func<T, bool> predicate)
    {
        for (int index = 0; index < list.Count; index++)
        {
            if (predicate(list[index]))
            {
                list.RemoveAt(index);
                return true;
            }
        }
        return false;
    }

    /// <summary>Replaces the first instance in <paramref name="list"/> that matches <paramref name="predicate"/>.</summary>
    /// <returns>True if an item was removed, false otherwise.</returns>
    public static bool ReplaceFirst<T>(this IList<T> list, Func<T, bool> predicate, T value)
    {
        for (int index = 0; index < list.Count; index++)
        {
            if (predicate(list[index]))
            {
                list[index] = value;
                return true;
            }
        }
        return false;
    }
}
