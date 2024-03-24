using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.TextCommand.Input.Result;
/// <summary>
/// Parsing result contains multiple value, and provide an <see cref="List{T}"/>
/// </summary>
/// <remarks>
/// do not use default to create this struct
/// </remarks>
/// <typeparam name="T">Type of collection item</typeparam>
public readonly ref struct ArgResultsList<T>
{
    internal static ArgResultsList<T> Empty => new(null, ref Unsafe.NullRef<ArgRawResultInfo>());

    private readonly List<T>? _values;
    private readonly ref ArgRawResultInfo _rawResultInfoStart;

    /// <summary>
    /// Values
    /// </summary>
    public List<T> Values => _values ?? []; // Lazy init to avoid extra alloc when error

    internal Span<ArgRawResultInfo> RawInfos
        => _values is null ? [] : MemoryMarshal.CreateSpan(ref _rawResultInfoStart, _values.Count);

    internal ArgResultsList(Span<ArgRawResultInfo> allocated) :
        this(new List<T>(allocated.Length), ref MemoryMarshal.GetReference(allocated))
    {
        Debug.Assert(allocated.Length > 0);
    }

    private ArgResultsList(List<T>? list, ref ArgRawResultInfo start)
    {
        _values = list;
        _rawResultInfoStart = ref start;
    }
}
