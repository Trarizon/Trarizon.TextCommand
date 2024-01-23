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
    internal readonly Span<ArgIndex> Indices;
    internal int CountOfEscapes;

    internal StringInputRest(ReadOnlySpan<char> source, Span<ArgIndex> indices, int countOfEscapes)
    {
        Source = source;
        Indices = indices;
        CountOfEscapes = countOfEscapes;
    }
}
