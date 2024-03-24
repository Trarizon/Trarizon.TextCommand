using System.ComponentModel;
using System.Runtime.InteropServices;
using Trarizon.TextCommand.Utilities;

namespace Trarizon.TextCommand.Input;
/// <summary>
/// This type is designed to avoid excessice string allocation
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public ref struct StringInputMatcher
{
    private readonly ReadOnlySpan<char> _input;
    // slice
    // Escaped
    private readonly Span<ArgIndex> _indexes;
    private int _countOfEscaped;

#if DEBUG
    private readonly List<bool> _isCalled = [];
    private readonly List<string> _calledCache = [];
#endif

    /// <summary>
    /// Get string split by index
    /// </summary>
    /// <remarks>
    /// Every time call the getter will create a string
    /// So this is for one-time use (exactly for compiler generating list pattern)
    /// </remarks>
    public ReadOnlySpan<char> this[int index]
    {
        get {
#if DEBUG
            if (index >= _isCalled.Count)
                CollectionsMarshal.SetCount(_isCalled, index + 1);
            if (_isCalled[index])
                throw new InvalidOperationException("Called twice");
            _isCalled[index] = true;
#endif

            var argIndex = _indexes[index];
            // Requires escape
            if (argIndex.Kind == ArgIndexKind.Escaped) {
                var (start, length) = argIndex.EscapedRange;
                var unescaped = StringUtil.UnescapeToString(_input.Slice(start, length));
                _countOfEscaped--;
#if DEBUG
                _calledCache.Add(unescaped);
#endif
                return unescaped;
            }
            // Slice
            else {
                var (start, length) = argIndex.SliceRange;
#if DEBUG
                _calledCache.Add(_input.Slice(start, length).ToString());
#endif
                return _input.Slice(start, length);
            }
        }
    }

    /// <summary>
    /// Get string splits by range
    /// </summary>
    /// <remarks>
    /// Exactly for compiler generating list pattern again, to get rest values
    /// </remarks>
    public readonly StringInputRest this[Range range] => new(_input, _indexes[range], _countOfEscaped);

    /// <summary>
    /// Count of input argument
    /// </summary>
    public readonly int Length => _indexes.Length;

    /// <summary>
    /// Create
    /// </summary>
    public StringInputMatcher(ReadOnlySpan<char> input)
    {
        _input = input;

        _countOfEscaped = 0;
        List<ArgIndex> ranges = [];
        for (int i = 0; i < _input.Length; i++) {
            char c = _input[i];
            if (char.IsWhiteSpace(c))
                continue;

            if (c == '"') {
                ++i;
                var end = FindSingleQuote(input, i);
                ranges.Add(ArgIndex.Escaped(i, end - i));
                i = end;
                _countOfEscaped++;
            }
            else {
                var end = FindWhiteSpace(input, i + 1);
                ranges.Add(ArgIndex.Slice(i, end - i));
                i = end;
            }
        }

        _indexes = CollectionsMarshal.AsSpan(ranges);

        static int FindSingleQuote(ReadOnlySpan<char> input, int start)
        {
            for (; start < input.Length; start++) {
                if (input[start] == '"') {
                    // ""
                    if (start + 1 < input.Length && input[start + 1] == '"') {
                        start++;
                    }
                    // end
                    else {
                        break;
                    }
                }
            }
            // if out of range, this is _input.Length
            // SplitArgs will not directly access _input[i];
            return start;
        }

        static int FindWhiteSpace(ReadOnlySpan<char> input, int start)
        {
            while (start < input.Length && !char.IsWhiteSpace(input[start]))
                start++;
            return start;
        }
    }
}
