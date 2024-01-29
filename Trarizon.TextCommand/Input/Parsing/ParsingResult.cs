using System.ComponentModel;

namespace Trarizon.TextCommand.Input.Parsing;
/// <summary>
/// Parsing result from provider
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public readonly struct ParsingResult<T>
{
    private readonly T? _value;
    internal readonly ParsingErrorSimple Error;

    /// <summary>
    /// The value of parsing, maybe null when not success
    /// </summary>
    public T Value => _value!;

    /// <summary>
    /// Indicate if the parsing is successful
    /// </summary>
    public bool IsSuccess => Error._errorKind == ParsingErrorKind.NoError;

    private ParsingResult(T? value, ParsingErrorSimple error)
    {
        _value = value;
        Error = error;
    }

    internal ParsingResult(T value) : this(value, default) { }

    internal ParsingResult(ParsingErrorSimple error) : this(default, error) { }
}
