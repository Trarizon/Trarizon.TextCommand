using System.Diagnostics.CodeAnalysis;
using Trarizon.TextCommand.Input;

namespace Trarizon.TextCommand.Parsers;
public readonly struct DelegateParser<T>(ArgParsingDelegate<T> parser) : IArgParser<T>
{
    public bool TryParse(InputArg input, [MaybeNullWhen(false)] out T result)
        => parser.Invoke(input, out result);
}
