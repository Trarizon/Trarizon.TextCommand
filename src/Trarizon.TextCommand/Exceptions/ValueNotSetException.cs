using System.Diagnostics.CodeAnalysis;

namespace Trarizon.TextCommand.Exceptions;
/// <summary>
/// A required parameter is not set
/// </summary>
/// <param name="parameter">Command parameter name</param>
public sealed class ValueNotSetException(string parameter) : Exception
{
    /// <summary>
    /// Command parameter name
    /// </summary>
    public string Parameter => parameter;

    /// <inheritdoc />
    public override string Message => $"Argument of '{parameter}' does not set";

    /// <summary>
    /// Create a exception and throw
    /// </summary>
    [DoesNotReturn]
    public static void Throw(string parameter)
        => throw new ValueNotSetException(parameter);
}
