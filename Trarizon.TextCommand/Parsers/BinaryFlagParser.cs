namespace Trarizon.TextCommand.Parsers;
public readonly struct BinaryFlagParser<T>(T trueValue, T falseValue) : IArgFlagParser<T>
{
    public T Parse(bool flag) => flag ? trueValue : falseValue;
}
