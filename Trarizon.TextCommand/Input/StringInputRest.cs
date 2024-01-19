using System.ComponentModel;

namespace Trarizon.TextCommand.Input;
[EditorBrowsable(EditorBrowsableState.Never)]
public ref struct StringInputRest
{
    internal readonly ReadOnlySpan<char> Source;
    /// <summary>
    /// slice or
    /// escaped
    /// </summary>
    internal readonly Span<ArgIndex> Indexes;
    internal int CountOfEscapes;

    internal StringInputRest(ReadOnlySpan<char> source, Span<ArgIndex> indexes, int countOfEscapes)
    {
        Source = source;
        Indexes = indexes;
        CountOfEscapes = countOfEscapes;
    }
}
