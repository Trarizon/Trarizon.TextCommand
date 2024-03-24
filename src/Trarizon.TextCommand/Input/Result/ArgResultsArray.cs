using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.TextCommand.Input.Result;
/// <summary>
/// Parsing result contains multiple value, and provide an <typeparamref name="T"/><c>[]</c>
/// </summary>
/// <remarks>
/// do not use default to create this struct
/// </remarks>
/// <typeparam name="T">Type of collection item</typeparam>
public readonly ref struct ArgResultsArray<T>
{
    internal static ArgResultsArray<T> Empty => new([], ref Unsafe.NullRef<ArgRawResultInfo>());

    private readonly T[] _values;
    private readonly ref ArgRawResultInfo _rawResultInfoStart;

    /// <summary>
    /// Result values
    /// </summary>
    public T[] Values => _values ?? [];

    internal Span<ArgRawResultInfo> RawInfos => Unsafe.IsNullRef(in _rawResultInfoStart) ? [] : MemoryMarshal.CreateSpan(ref _rawResultInfoStart, _values.Length);

    internal ArgResultsArray(Span<ArgRawResultInfo> allocatedSpace) :
        this(new T[allocatedSpace.Length], ref MemoryMarshal.GetReference(allocatedSpace))
    {
        Debug.Assert(allocatedSpace.Length > 0);
    }

    private ArgResultsArray(T[] values, ref ArgRawResultInfo rawResultInfoStart)
    {
        _values = values;
        _rawResultInfoStart = ref rawResultInfoStart;
    }
}
