using System.Diagnostics;

namespace Trarizon.TextCommand.Input;
#if DEBUG
[DebuggerDisplay("{DebuggerDisplayString}")]
#endif
internal readonly struct ArgIndex
{
    private readonly int _index;
    private readonly int _length;

#if DEBUG
    public string DebuggerDisplayString => Kind switch {
        ArgIndexKind.Slice => $"Slice: {SliceRange}",
        ArgIndexKind.Flag => "Flag",
        ArgIndexKind.FromCached => $"Cached: {CachedIndex}",
        ArgIndexKind.Escaped => $"Escaped: {EscapedRange}",
        _ => "?",
    };
#endif

    public ArgIndexKind Kind => (_index, _length) switch {
        ( >= 0, >= 0) => ArgIndexKind.Slice,
        ( >= 0, < 0) => ArgIndexKind.FromCached,
        ( < 0, >= 0) => ArgIndexKind.Escaped,
        ( < 0, < 0) => ArgIndexKind.Flag,
    };

    public (int Start, int Length) SliceRange => (_index, _length);

    public int CachedIndex => _index;

    public (int Start, int Length) EscapedRange => (~_index, _length);

    private ArgIndex(int index, int length) { _index = index; _length = length; }

    public static readonly ArgIndex Flag = new(-1, -1);

    public static ArgIndex Slice(int start, int length)
    {
        Debug.Assert(start >= 0 && length >= 0);
        return new ArgIndex(start, length);
    }

    public static ArgIndex FromCached(int index)
    {
        Debug.Assert(index >= 0);
        return new ArgIndex(index, -1);
    }

    public static ArgIndex Escaped(int start, int length)
    {
        Debug.Assert(start >= 0 && length >= 0);
        return new ArgIndex(~start, length);
    }
}

internal enum ArgIndexKind
{
    /// <summary>
    /// The index is a normal Range
    /// </summary>
    Slice,
    /// <summary>
    /// The value is a flag, thus indicates no range
    /// </summary>
    Flag,
    /// <summary>
    /// The value is cached
    /// </summary>
    FromCached,
    /// <summary>
    /// The index is a range that indicated a string requires unescape
    /// </summary>
    Escaped,
}
