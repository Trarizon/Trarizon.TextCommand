using System.ComponentModel;

namespace Trarizon.TextCommand.Input;
/// <summary>
/// A span for <see cref="StringInputMatcher"/>
/// </summary>
/// <remarks>
/// Mainly for compiler generating list pattern slice pattern
/// </remarks>
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
