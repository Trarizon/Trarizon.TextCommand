namespace Trarizon.TextCommand.Input.Parsing;
internal readonly struct ParsingError
{
    internal readonly ParsingErrorSimple _error;
    internal readonly Type _parsedType;
    internal readonly string _parameterName;

    internal ParsingError(ParsingErrorSimple error, Type parsedType, string parameterName)
    {
        _error = error;
        _parsedType = parsedType;
        _parameterName = parameterName;
    }
}
