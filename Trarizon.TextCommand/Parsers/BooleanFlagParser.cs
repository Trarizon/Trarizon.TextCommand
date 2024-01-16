namespace Trarizon.TextCommand.Parsers;
public readonly struct BooleanFlagParser : IArgFlagParser<bool>
{
    public bool Parse(bool flag) => flag;
}
