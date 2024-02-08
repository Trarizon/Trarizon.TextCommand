using System.Diagnostics.CodeAnalysis;
using Trarizon.TextCommand.Input;

namespace Trarizon.TextCommand.Parsers;
public readonly struct NullableParser<T, TParser>(TParser parser) : IArgParser<T?>
    where T : struct
    where TParser : IArgParser<T>
{
    public bool TryParse(InputArg input, [MaybeNullWhen(false)] out T? result)
    {
        if(parser.TryParse(input, out var tmp)) {
            result = tmp;
            return true;
        }
        else {
            result = default;
            return false;
        }
    }
}
