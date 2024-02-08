namespace Trarizon.TextCommand.Input;
public ref struct InputArg
{
    private readonly ReadOnlySpan<char> _rawInputSpan;
    private string? _rawInputString;

    internal InputArg(ReadOnlySpan<char> fromSpan) => _rawInputSpan = fromSpan;

    internal InputArg(string fromString) => _rawInputString = fromString;

    public readonly bool IsFromString => _rawInputString != null;

    public string RawInput => _rawInputString ??= _rawInputSpan.ToString();

    public readonly ReadOnlySpan<char> RawInputSpan => _rawInputSpan.Length == 0 ? _rawInputString.AsSpan() : _rawInputSpan;

    public override string ToString() => RawInput;
}
