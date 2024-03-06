using System.Diagnostics.CodeAnalysis;
using Trarizon.TextCommand.Input;

namespace Trarizon.TextCommand.Parsers;
/// <summary>
/// Parse using delegate
/// </summary>
/// <typeparam name="T">Result value</typeparam>
/// <param name="parser">The parser</param>
public readonly struct DelegateParser<T>(ArgParsingDelegate<T> parser) : IArgParser<T>
{
    /// <inheritdoc />
    public bool TryParse(InputArg input, [MaybeNullWhen(false)] out T result)
        => parser.Invoke(input, out result);
}

/// <summary>
/// Parse using delegate
/// </summary>
/// <typeparam name="T">Result value</typeparam>
/// <param name="parser">The parser</param>
public readonly struct DelegateFlagParser<T>(ArgFlagParsingDelegate<T> parser) : IArgFlagParser<T>
{
    /// <inheritdoc />
    public T Parse(bool flag) => parser.Invoke(flag);
}
