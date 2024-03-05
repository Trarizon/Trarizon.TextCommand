using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.TextCommand.Input.Result;
public readonly ref struct ArgResultsUnmanaged<T> where T : unmanaged
{
    private readonly ref T _valueStart;
    private readonly int _length;

    public Span<T> Values => Unsafe.IsNullRef(in _valueStart) ? [] : MemoryMarshal.CreateSpan(ref _valueStart, _length);

    internal Span<ArgRawResultInfo> RawInfos => MemoryMarshal.CreateSpan(
        ref Unsafe.As<T, ArgRawResultInfo>(ref Unsafe.Add(ref _valueStart, _length)),
        _length);

    internal ArgResultsUnmanaged(Span<ArgResult<T>> allocatedSpace)
    {
        // Change allocated space layout [(value,index,kind), ..]
        // to [value, ..] [index, ..] [kind, ..];
        _length = allocatedSpace.Length;
        _valueStart = ref Unsafe.As<ArgResult<T>, T>(ref MemoryMarshal.GetReference(allocatedSpace));
    }
}
