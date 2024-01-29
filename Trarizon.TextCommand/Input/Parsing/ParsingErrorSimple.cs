namespace Trarizon.TextCommand.Input.Parsing;
public readonly struct ParsingErrorSimple
{
    internal readonly ArgIndex _index;
    internal readonly ParsingErrorKind _errorKind;

    private ParsingErrorSimple(ArgIndex index, ParsingErrorKind errorKind)
    {
        _index = index;
        _errorKind = errorKind;
    }

    internal ParsingErrorSimple(ArgIndex index) : this(index, ParsingErrorKind.ParsingFailed) { }

    internal static ParsingErrorSimple NotSet => new(default, ParsingErrorKind.ParameterNotSet);
}
