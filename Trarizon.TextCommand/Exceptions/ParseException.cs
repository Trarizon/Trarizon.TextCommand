using System.Diagnostics.CodeAnalysis;

namespace Trarizon.TextCommand.Exceptions;
public sealed class ParseException(string parameter, Type parsedType) : Exception
{
    public string Parameter => parameter;

    public override string Message => $"Cannot parse value of {parameter} to {parsedType}";

    /// <summary>
    /// Create a new <see cref="ParseException"/> and throw
    /// </summary>
    [DoesNotReturn]
    public static void Throw(string parameter, Type parsedType)
        => throw new ParseException(parameter, parsedType);
}
