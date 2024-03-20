using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;
internal static class QueryExtensions
{
    public static bool TryFirst<T>(this ImmutableArray<T> source, Func<T, bool> predicate, [MaybeNullWhen(false)] out T value)
    {
        var enumerator = source.GetEnumerator();

        while (enumerator.MoveNext()) {
            var current = enumerator.Current;
            if (predicate(current)) {
                value = current;
                return true;
            }
        }
        value = default;
        return false;
    }

    public static bool TrySingleOrNone<T>(this IEnumerable<T> source, Func<T, bool> predicate, [MaybeNullWhen(false)] out T value, T? defaultValue = default!)
        => source.TryPredicateSingleInternal(predicate, out value, defaultValue, true);

    private static bool TryPredicateSingleInternal<T>(this IEnumerable<T> source, Func<T, bool> predicate, [MaybeNullWhen(false)] out T value, T? defaultValue, bool resultWhenNotFound)
    {
        using var enumerator = source.GetEnumerator();

        bool find = false;
        T current = default!;
        while (enumerator.MoveNext()) {
            current = enumerator.Current;
            if (predicate(current)) {
                if (find) {
                    // Multiple
                    value = defaultValue;
                    return false;
                }
                find = true;
            }
        }

        // Single
        if (find) {
            value = current;
            return true;
        }
        // Not found
        else {
            value = defaultValue;
            return resultWhenNotFound;
        }
    }

    public static bool TryAt(this string source, int index, out char value)
    {
        if (index < 0 || index >= source.Length) {
            value = default;
            return false;
        }

        value = source[index];
        return true;
    }

    public static IEnumerable<TResult> SelectNonException<T, TResult>(this IEnumerable<T> source, Func<T, TResult> select)
    {
        foreach (var item in source) {
            TResult val;
            try {
                val = select(item);
            } catch (Exception) {
                continue;
            }
            yield return val;
        }
    }

    public static ImmutableArray<T> EmptyIfDefault<T>(this ImmutableArray<T> source)
    {
        if (source.IsDefault)
            return ImmutableArray<T>.Empty;
        return source;
    }

    public static IEnumerable<T> OfNotNull<T>(this IEnumerable<T?> source) where T : class
    {
        foreach (var item in source) {
            if (item is not null)
                yield return item;
        }
    }

    public static IEnumerable<T> SingletonCollection<T>(this T value)
    {
        yield return value;
    }
}
