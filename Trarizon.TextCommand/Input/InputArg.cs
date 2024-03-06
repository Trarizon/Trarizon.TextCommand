namespace Trarizon.TextCommand.Input;
/// <summary>
/// Raw input context
/// </summary>
public ref struct InputArg
{
    private readonly ReadOnlySpan<char> _rawInputSpan;
    private string? _rawInputString;

    internal InputArg(ReadOnlySpan<char> fromSpan) => _rawInputSpan = fromSpan;

    internal InputArg(string fromString) => _rawInputString = fromString;

    /// <summary>
    /// Is this value contains a string
    /// </summary>
    public readonly bool IsFromString => _rawInputSpan.Length == 0;

    /// <summary>
    /// Raw input in <see cref="string"/>, new <see cref="string"/> may be created if raw input not <see cref="IsFromString"/>
    /// </summary>
    public string RawInput => _rawInputString ??= _rawInputSpan.ToString();

    /// <summary>
    /// Raw input in <see cref="Span{T}"/>
    /// </summary>
    public readonly ReadOnlySpan<char> RawInputSpan => _rawInputSpan.Length == 0 ? _rawInputString.AsSpan() : _rawInputSpan;

    /// <inheritdoc />
    public override string ToString() => RawInput;
}
