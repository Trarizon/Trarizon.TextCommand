using System.Diagnostics.CodeAnalysis;
using Trarizon.TextCommand.Input;

namespace Trarizon.TextCommand.Parsers;
public interface IArgParser<T>
{
    bool TryParse(InputArg input, [MaybeNullWhen(false)] out T result);
}

public interface IArgFlagParser<T>
{
    T Parse(bool flag);
}
