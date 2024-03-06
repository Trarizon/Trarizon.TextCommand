using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.TextCommand.Input.Result;
/// <summary>
/// Parsing result contains multiple value, and provide an <see cref="List{T}"/>
/// </summary>
/// <typeparam name="T">Type of collection item</typeparam>
public readonly ref struct ArgResultsList<T>
{
    private static readonly List<T> EmptyList = [];

    private readonly List<T> _values;
    private readonly ref ArgRawResultInfo _rawResultInfoStart;

    /// <summary>
    /// Values
    /// </summary>
    public List<T> Values => _values;

    internal Span<ArgRawResultInfo> RawInfos => Unsafe.IsNullRef(in _rawResultInfoStart) ? [] : MemoryMarshal.CreateSpan(ref _rawResultInfoStart, _values.Count);

    internal ArgResultsList(Span<ArgRawResultInfo> allocated)
    {
        if (allocated.Length == 0) {
            _values = EmptyList;
        }
        else {
            _values = new List<T>(allocated.Length);
            CollectionsMarshal.SetCount(_values, allocated.Length);
            _rawResultInfoStart = ref MemoryMarshal.GetReference(allocated);
        }
    }
}
