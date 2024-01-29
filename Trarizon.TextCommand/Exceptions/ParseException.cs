using System.Diagnostics.CodeAnalysis;

namespace Trarizon.TextCommand.Exceptions;
public sealed class ParseException(string parameter, Type parsedType) : Exception
{
    public string Parameter => parameter;

    public override string Message => $"Cannot parse value of {parameter} to {parsedType}";

    [DoesNotReturn]
    public static void Throw(string parameter, Type parsedType)
        => throw new ParseException(parameter, parsedType));
}
