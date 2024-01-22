using System.Diagnostics.CodeAnalysis;

namespace Trarizon.TextCommand.Parsers;
public readonly struct NullableParser<TParser, T>(TParser parser) : IArgParser<T?>
    where TParser : IArgParser<T>
    where T : struct
{
    public bool TryParse(ReadOnlySpan<char> rawArg, [MaybeNullWhen(false)] out T? result)
    {
        if (parser.TryParse(rawArg, out var tmp)) {
            result = tmp;
            return true;
        }
        else {
            result = default;
            return false;
        }
    }
}
