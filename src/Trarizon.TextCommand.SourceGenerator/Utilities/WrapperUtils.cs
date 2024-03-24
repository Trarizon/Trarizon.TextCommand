using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.TextCommand.SourceGenerator.Utilities;
internal static class WrapperUtils
{
    public static Optional<T> Optional<T>(T value) => new(value);

    public static Optional<T> OptionalNotNull<T>(T? value) where T : class => value is null ? default : new(value); 

    public static bool TryGetValue<T>(this Optional<T> optional, [MaybeNullWhen(false)] out T value)
    {
        value = optional.Value;
        return optional.HasValue;
    }
}
