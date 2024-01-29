using System.Runtime.InteropServices;

namespace Trarizon.TextCommand.Input.Parsing;
public readonly ref struct ParsingResultsArray<T>
{
    private readonly T[] _results;
    private readonly ref ParsingErrorSimple _errorStart;

    internal ParsingResultsArray(Span<ParsingErrorSimple> errors)
    {
        _results = new T[errors.Length];
        _errorStart = ref MemoryMarshal.GetReference(errors);
    }

    public T[] Results => _results;
    internal Span<ParsingErrorSimple> Errors => MemoryMarshal.CreateSpan(ref _errorStart, _results.Length);
}
