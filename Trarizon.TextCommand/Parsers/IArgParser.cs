using System.Diagnostics.CodeAnalysis;

namespace Trarizon.TextCommand.Parsers;
public interface IArgParser<T>
{
    bool TryParse(ReadOnlySpan<char> rawArg, [MaybeNullWhen(false)] out T result);
}

public interface IArgFlagParser<T>
{
    T Parse(bool flag);
}
