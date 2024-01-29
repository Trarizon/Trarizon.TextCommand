using System.Runtime.InteropServices;

namespace Trarizon.TextCommand.Input.Parsing;
public readonly ref struct ParsingResultsList<T>
{
    private readonly List<T> _results;
    private readonly ref ParsingErrorSimple _errorStart;

    internal ParsingResultsList(Span<ParsingErrorSimple> errors)
    {
        _results = new List<T>(errors.Length);
        _errorStart = ref MemoryMarshal.GetReference(errors);
    }

    public List<T> Results => _results;
    internal Span<ParsingErrorSimple> Errors => MemoryMarshal.CreateSpan(ref _errorStart, _results.Count);
}
