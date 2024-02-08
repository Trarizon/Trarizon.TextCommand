using System.Diagnostics.CodeAnalysis;
using Trarizon.TextCommand.Input;

namespace Trarizon.TextCommand.Parsers;
public readonly struct EnumParser<T>(bool caseSensitive) : IArgParser<T> where T : struct, Enum
{
    public bool TryParse(InputArg input, [MaybeNullWhen(false)] out T result)
        => Enum.TryParse(input.RawInputSpan, !caseSensitive, out result);
}
