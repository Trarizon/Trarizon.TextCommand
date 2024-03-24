using System.Diagnostics.CodeAnalysis;
using Trarizon.TextCommand.Input;

namespace Trarizon.TextCommand.Parsers.Wrapped;
/// <summary>
/// Wrapper a parser for <see cref="Nullable{T}"/>
/// </summary>
/// <typeparam name="T">Original value type</typeparam>
/// <typeparam name="TParser">Parser type</typeparam>
/// <param name="parser">Wrapped parser</param>
public readonly struct NullableParser<T, TParser>(TParser parser) : IArgParser<T?>
    where T : struct
    where TParser : IArgParser<T>
{
    /// <inheritdoc />
    public bool TryParse(InputArg input, [MaybeNullWhen(false)] out T? result)
    {
        if (parser.TryParse(input, out var tmp))
        {
            result = tmp;
            return true;
        }
        else
        {
            result = default;
            return false;
        }
    }
}
