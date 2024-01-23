using System.Diagnostics.CodeAnalysis;

namespace Trarizon.TextCommand.Exceptions;
public sealed class ParseException(string parameter, Type targetType) : Exception
{
    public string Parameter => parameter;

    public override string Message => $"Cannot parse value of {parameter} to {targetType}";

    [DoesNotReturn]
    public static void Throw<TTarget>(string parameter)
        => throw new ParseException(parameter, typeof(TTarget));
}
