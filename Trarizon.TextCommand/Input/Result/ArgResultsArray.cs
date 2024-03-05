using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Trarizon.TextCommand.Input.Result;
public readonly ref struct ArgResultsArray<T>
{
    private readonly T[] _values;
    private readonly ref ArgRawResultInfo _rawResultInfoStart;

    public T[] Values => _values ?? [];

    internal Span<ArgRawResultInfo> RawInfos => MemoryMarshal.CreateSpan(ref _rawResultInfoStart, _values.Length);

    internal ArgResultsArray(Span<ArgRawResultInfo> allocatedSpace)
    {
        Debug.Assert(allocatedSpace.Length > 0);
        _values = new T[allocatedSpace.Length];
        _rawResultInfoStart = ref MemoryMarshal.GetReference(allocatedSpace);
    }
}
