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

    // Every time call the getter will create a string
    // So this is for one-time use
    public string this[int index]
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
            if (argIndex.Kind == ArgIndexKind.Escaped) { // Requires escape
                var (start, length) = argIndex.EscapedRange;
                var unescaped = StringUtil.UnescapeToString(_input.Slice(start, length));
                _countOfEscaped--;
#if DEBUG
                _calledCache.Add(unescaped);
#endif
                return unescaped;
            }
            else { // Slice
                var (start, length) = argIndex.SliceRange;
#if DEBUG
                _calledCache.Add(_input.AsSpan(start, length).ToString());
#endif
                return new(_input.Slice(start, length));
            }
        }
    }

    // Get the rest part of string
    public readonly StringInputRest this[Range range] => new(_input, _indexes[range], _countOfEscaped);

    public readonly int Length => _indexes.Length;

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
