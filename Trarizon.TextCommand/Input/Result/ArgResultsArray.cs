using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.TextCommand.Input.Result;
/// <summary>
/// Parsing result contains multiple value, and provide an <typeparamref name="T"/><c>[]</c>
/// </summary>
/// <typeparam name="T">Type of collection item</typeparam>
public readonly ref struct ArgResultsArray<T>
{
    private readonly T[] _values;
    private readonly ref ArgRawResultInfo _rawResultInfoStart;

    /// <summary>
    /// Result values
    /// </summary>
    public T[] Values => _values ?? [];

    internal Span<ArgRawResultInfo> RawInfos => Unsafe.IsNullRef(in _rawResultInfoStart) ? [] : MemoryMarshal.CreateSpan(ref _rawResultInfoStart, _values.Length);

    internal ArgResultsArray(Span<ArgRawResultInfo> allocatedSpace)
    {
        Debug.Assert(allocatedSpace.Length > 0);
        _values = new T[allocatedSpace.Length];
        _rawResultInfoStart = ref MemoryMarshal.GetReference(allocatedSpace);
    }
}
