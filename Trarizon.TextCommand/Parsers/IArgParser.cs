using System.Diagnostics.CodeAnalysis;
using Trarizon.TextCommand.Input;

namespace Trarizon.TextCommand.Parsers;
/// <summary>
/// Parse string input into value
/// </summary>
/// <typeparam name="T">Result value</typeparam>
public interface IArgParser<T>
{
    /// <summary>
    /// Try parse a string input into specific type
    /// </summary>
    /// <returns>Is parsing success</returns>
    bool TryParse(InputArg input, [MaybeNullWhen(false)] out T result);
}

/// <summary>
/// Parse flag input into value
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IArgFlagParser<T>
{
    /// <summary>
    /// Parse flag into specific type
    /// </summary>
    /// <param name="flag">Input</param>
    /// <returns>Result</returns>
    T Parse(bool flag);
}
