using System.Diagnostics.CodeAnalysis;
using Trarizon.TextCommand.Input;

namespace Trarizon.TextCommand.Parsers.Wrapped;
/// <summary>
/// Convert result of parser into another type
/// </summary>
/// <remarks>
/// This wrapper is mainly for multi-value, as we cannot implicit cast an array into another type
/// </remarks>
/// <typeparam name="T">Source type</typeparam>
/// <typeparam name="TResult">Target type</typeparam>
/// <typeparam name="TParser">Wrapped parser</typeparam>
/// <param name="convert">converter</param>
/// <param name="parser">wrapped parser</param>
public readonly struct ConversionParser<T, TResult, TParser>(TParser parser, Func<T?, TResult> convert) : IArgParser<TResult> where TParser : IArgParser<T>
{
    /// <inheritdoc />
    public bool TryParse(InputArg input, [MaybeNullWhen(false)] out TResult result)
    {
        bool rtn = parser.TryParse(input, out var res);
        result = convert(res);
        return rtn;
    }
}
