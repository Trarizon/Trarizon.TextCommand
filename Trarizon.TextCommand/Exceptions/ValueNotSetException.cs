using System.Diagnostics.CodeAnalysis;

namespace Trarizon.TextCommand.Exceptions;
public sealed class ValueNotSetException(string parameter) : Exception
{
    public string Parameter => parameter;

    public override string Message => $"Argument of '{parameter}' does not set";

    [DoesNotReturn]
    public static void Throw(string parameter)
        => throw new ValueNotSetException(parameter);
}
