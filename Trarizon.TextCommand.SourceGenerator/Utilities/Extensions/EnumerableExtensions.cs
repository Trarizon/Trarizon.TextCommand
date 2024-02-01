using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;
internal static class EnumerableExtensions
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

    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source) => source ?? [];

    public static bool TrySingle<T>(this IEnumerable<T> source, Func<T, bool> predicate, [MaybeNullWhen(false)] out T value, T? defaultValue = default!)
        => source.TryPredicateSingleInternal(predicate, out value, defaultValue, false);

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

}
