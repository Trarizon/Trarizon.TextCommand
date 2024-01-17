using System.Diagnostics.CodeAnalysis;

namespace Trarizon.TextCommand.Parsers;
public readonly struct DelegateParser<T>(ArgParsingDelegate<T> parser) : IArgParser<T>
{
    public bool TryParse(ReadOnlySpan<char> rawArg, [MaybeNullWhen(false)] out T result)
        => parser.Invoke(rawArg, out result);
}
