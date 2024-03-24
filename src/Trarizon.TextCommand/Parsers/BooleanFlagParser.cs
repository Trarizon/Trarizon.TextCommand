namespace Trarizon.TextCommand.Parsers;
/// <summary>
/// Return the flag as itself
/// </summary>
public readonly struct BooleanFlagParser : IArgFlagParser<bool>
{
    /// <inheritdoc />
    public bool Parse(bool flag) => flag;
}
