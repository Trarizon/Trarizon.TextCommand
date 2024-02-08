using System.Diagnostics.CodeAnalysis;
using Trarizon.TextCommand.Input;

namespace Trarizon.TextCommand.Parsers;
public readonly struct ParsableParser<T>(IFormatProvider? formatProvider) : IArgParser<T> where T : ISpanParsable<T>
{
    public bool TryParse(InputArg input, [MaybeNullWhen(false)] out T result)
    {
        // Specialized for string, as TryParse(ROS<char>) for string will create new string
        if (typeof(T) == typeof(string)) {
            return T.TryParse(input.RawInput, formatProvider, out result);
        }

        return T.TryParse(input.RawInputSpan, formatProvider, out result);
    }
}
