using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;
internal static class AttributeDataExtensions
{
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? GetNamedArgument<T>(this AttributeData attribute, string parameterName, T? defaultValue = default)
        => attribute.NamedArguments.TryFirst(kv => kv.Key == parameterName, out var first)
        ? (T)first.Value.Value!
        : defaultValue;

    public static ImmutableArray<T> GetNamedArguments<T>(this AttributeData attribute, string parameterName)
    {
        if (attribute.NamedArguments.TryFirst(kv => kv.Key == parameterName, out var first)) {
            var constants = first.Value.Values;
            if (constants.Length == 0)
                return ImmutableArray<T>.Empty;

            return constants.Select(constant => (T)constant.Value!).ToImmutableArray();
        }
        return default;
    }

    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static T? GetConstructorArgument<T>(this AttributeData attribute, int index, T? defaultValue = default)
        => attribute.ConstructorArguments is var args && index >= 0 && index < args.Length
        ? (T)args[index].Value!
        : defaultValue;

    public static ImmutableArray<T> GetConstructorArguments<T>(this AttributeData attribute, int index)
    {
        if (attribute.ConstructorArguments is var args && index >= 0 && index < args.Length) {
            var constants = args[index].Values;
            if (constants.Length == 0)
                return ImmutableArray<T>.Empty;

           return constants.Select(constant => (T)constant.Value!).ToImmutableArray();
        }
        return default;
    }
}
