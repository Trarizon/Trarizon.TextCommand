using System.Diagnostics.CodeAnalysis;

namespace Trarizon.TextCommand.Parsers;
public readonly struct NullableParser<T, TParser>(TParser parser) : IArgParser<T?>
    where T : struct
    where TParser : IArgParser<T>
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
