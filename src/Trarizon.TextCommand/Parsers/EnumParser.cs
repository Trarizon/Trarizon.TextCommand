using System.Diagnostics.CodeAnalysis;
using Trarizon.TextCommand.Input;

namespace Trarizon.TextCommand.Parsers;
/// <summary>
/// Parse enum
/// </summary>
/// <typeparam name="T">Result type</typeparam>
/// <param name="caseSensitive">Is case sensitive</param>
public readonly struct EnumParser<T>(bool caseSensitive) : IArgParser<T> where T : struct, Enum
{
    /// <inheritdoc />
    public bool TryParse(InputArg input, [MaybeNullWhen(false)] out T result)
        => Enum.TryParse(input.RawInputSpan, !caseSensitive, out result);
}
