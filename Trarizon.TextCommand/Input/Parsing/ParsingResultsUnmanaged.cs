using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trarizon.TextCommand.Input.Parsing;
/// <summary>
/// The parsing result of unmanaged <see cref="Span{T}"/>
/// </summary>
public readonly ref struct ParsingResultsUnmanaged<T> where T : unmanaged
{
    private readonly ref T _resultAreaStart;
    private readonly int _length;

    internal ParsingResultsUnmanaged(Span<ParsingResult<T>> allocatedSpace)
    {
        // Change allocated space layout [(value,index,kind), ..]
        // to [value, ..] [index, ..] [kind, ..];
        _length = allocatedSpace.Length;
        _resultAreaStart = ref Unsafe.As<ParsingResult<T>, T>(ref MemoryMarshal.GetReference(allocatedSpace));
    }

    public readonly Span<T> Results => MemoryMarshal.CreateSpan(ref _resultAreaStart, _length);
    internal readonly Span<ParsingErrorSimple> Errors => MemoryMarshal.CreateSpan(ref Unsafe.As<T, ParsingErrorSimple>(ref Unsafe.Add(ref _resultAreaStart, _length)), _length);
}
