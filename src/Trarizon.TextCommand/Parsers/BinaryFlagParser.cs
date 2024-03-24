namespace Trarizon.TextCommand.Parsers;
/// <summary>
/// flag ? <paramref name="trueValue"/> : <paramref name="falseValue"/>
/// </summary>
/// <typeparam name="T">Result type</typeparam>
/// <param name="trueValue">The value when flag is <see langword="true"/></param>
/// <param name="falseValue">The value when flag is <see langword="false"/></param>
public readonly struct BinaryFlagParser<T>(T trueValue, T falseValue) : IArgFlagParser<T>
{
    /// <inheritdoc />
    public T Parse(bool flag) => flag ? trueValue : falseValue;
}
