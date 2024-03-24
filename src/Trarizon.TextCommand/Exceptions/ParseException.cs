using System.Diagnostics.CodeAnalysis;

namespace Trarizon.TextCommand.Exceptions;
/// <summary>
/// Parse exception
/// </summary>
/// <param name="parameter">Command parameter name</param>
/// <param name="parsedType">Parsed type</param>
public sealed class ParseException(string parameter, Type parsedType) : Exception
{
    /// <summary>
    /// Command parameter name
    /// </summary>
    public string Parameter => parameter;

    /// <inheritdoc />
    public override string Message => $"Cannot parse value of {parameter} to {parsedType}";

    /// <summary>
    /// Create a new <see cref="ParseException"/> and throw
    /// </summary>
    [DoesNotReturn]
    public static void Throw(string parameter, Type parsedType)
        => throw new ParseException(parameter, parsedType);
}
