namespace Trarizon.TextCommand.Parsers;
public readonly struct DelegateFlagParser<T>(ArgFlagParsingDelegate<T> parser) : IArgFlagParser<T>
{
    public T Parse(bool flag) => parser.Invoke(flag);
}
