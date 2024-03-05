using System.Runtime.InteropServices;

namespace Trarizon.TextCommand.Input.Result;
public readonly ref struct ArgResultsList<T>
{
    private readonly List<T> _values;
    private readonly ref ArgRawResultInfo _rawResultInfoStart;

    public List<T> Values => _values ?? [];

    internal Span<ArgRawResultInfo> RawInfos => MemoryMarshal.CreateSpan(ref _rawResultInfoStart, _values.Count);

    internal ArgResultsList(Span<ArgRawResultInfo> allocated)
    {
        _values = new List<T>(allocated.Length);
        CollectionsMarshal.SetCount(_values, allocated.Length);
        _rawResultInfoStart = ref MemoryMarshal.GetReference(allocated);
    }
}
