using System.Diagnostics.CodeAnalysis;

namespace Trarizon.TextCommand.Parsers;
public readonly struct ParsableParser<T>(IFormatProvider? formatProvider) : IArgParser<T> where T : ISpanParsable<T>
{
    public bool TryParse(ReadOnlySpan<char> rawArg, [MaybeNullWhen(false)] out T result)
        => T.TryParse(rawArg, formatProvider, out result);
}
