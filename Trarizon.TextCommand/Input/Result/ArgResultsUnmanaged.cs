using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.TextCommand.Input.Result;
/// <summary>
/// Parsing result contains multiple unmanaged value, and provide an <see cref="Span{T}"/>
/// </summary>
/// <remarks>
/// do not use default to create this struct
/// </remarks>
/// <typeparam name="T">Type of collection item</typeparam>
public readonly ref struct ArgResultsUnmanaged<T> where T : unmanaged
{
    internal static ArgResultsUnmanaged<T> Empty => new(ref Unsafe.NullRef<T>(), 0);

    private readonly ref T _valueStart;
    private readonly int _length;

    /// <summary>
    /// Values
    /// </summary>
    public Span<T> Values => Unsafe.IsNullRef(in _valueStart) ? [] : MemoryMarshal.CreateSpan(ref _valueStart, _length);

    internal Span<ArgRawResultInfo> RawInfos => Unsafe.IsNullRef(in _valueStart) ? []
        : MemoryMarshal.CreateSpan(
            ref Unsafe.As<T, ArgRawResultInfo>(ref Unsafe.Add(ref _valueStart, _length)),
            _length);

    internal ArgResultsUnmanaged(Span<ArgResult<T>> allocatedSpace) :
        // Change allocated space layout [(value,info), ..]
        // to [value, ..] [info, ..];
        this(ref Unsafe.As<ArgResult<T>, T>(ref MemoryMarshal.GetReference(allocatedSpace)), allocatedSpace.Length)
    {
        Debug.Assert(allocatedSpace.Length > 0);
    }

    private ArgResultsUnmanaged(ref T valueStart, int length)
    {
        _valueStart = ref valueStart;
        _length = length;
    }
}
