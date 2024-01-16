using System.Diagnostics.CodeAnalysis;

namespace Trarizon.TextCommand.Parsers;
public readonly struct EnumParser<T>(bool caseSensitive) : IArgParser<T> where T : struct, Enum
{
    public bool TryParse(ReadOnlySpan<char> rawArg, [MaybeNullWhen(false)] out T result)
        => Enum.TryParse(rawArg, !caseSensitive, out result);
}
