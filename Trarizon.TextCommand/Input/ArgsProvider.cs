using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Trarizon.TextCommand.Input.Result;
using Trarizon.TextCommand.Parsers;

namespace Trarizon.TextCommand.Input;
/// <summary>
/// Parse and get arguments from input
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public readonly ref partial struct ArgsProvider
{
    internal readonly ReadOnlySpan<char> _sourceInput;
    internal readonly ReadOnlySpan<string> _sourceArray;

    // slice
    // from cached
    // flag
    private readonly Dictionary<string, ArgIndex> _optionDict;
    private readonly ReadOnlySpan<ArgIndex> _valueList;

    internal ArgsProvider(ReadOnlySpan<char> sourceInput, ReadOnlySpan<string> sourceArray, Dictionary<string, ArgIndex> dict, ReadOnlySpan<ArgIndex> list)
    {
        _sourceInput = sourceInput;
        _sourceArray = sourceArray;
        _optionDict = dict;
        _valueList = list;

        AssertCtor();
    }

    [Conditional("DEBUG")]
    private void AssertCtor()
    {
        // From array
        if (_sourceInput.Length == 0) {
            foreach (var (_, index) in _optionDict) {
                Debug.Assert(index.Kind is ArgIndexKind.FromCached);
            }
            foreach (var index in _valueList) {
                Debug.Assert(index.Kind is ArgIndexKind.FromCached);
            }
        }
        // From string
        else {
            foreach (var (_, index) in _optionDict) {
                Debug.Assert(index.Kind is ArgIndexKind.Slice or ArgIndexKind.FromCached or ArgIndexKind.Flag);
            }
            foreach (var index in _valueList) {
                Debug.Assert(index.Kind is ArgIndexKind.Slice or ArgIndexKind.FromCached or ArgIndexKind.Flag);
            }
        }
    }

    private bool TryParseArg<T, TParser>(ArgIndex index, TParser parser, [MaybeNullWhen(false)] out T result) where TParser : IArgParser<T>
    {
        // Slice
        if (index.Kind == ArgIndexKind.Slice) {
            var (start, length) = index.SliceRange;
            return parser.TryParse(new(_sourceInput.Slice(start, length)), out result);
        }
        // From cached
        else {
            Debug.Assert(index.Kind is ArgIndexKind.FromCached);
            return parser.TryParse(new(_sourceArray[index.CachedIndex]), out result);
        }
    }

    private void ParseArgs<T, TParser>(ReadOnlySpan<ArgIndex> indices, TParser parser, Span<T> valuesSpan, Span<ArgRawResultInfo> rawInfosSpan) where TParser : IArgParser<T>
    {
        for (int i = 0; i < indices.Length; i++) {
            var argIndex = indices[i];
            var resultKind = TryParseArg(argIndex, parser, out valuesSpan[i]!)
                ? ArgResultKind.NoError
                : ArgResultKind.ParsingFailed;
            rawInfosSpan[i] = new(argIndex, resultKind);
        }
    }

    /// <returns><see cref="ArgIndex.Flag"/> as <see langword="false"/></returns>
    private ArgIndex? GetRawOption(string key)
    {
        if (_optionDict.TryGetValue(key, out var index)) {
            // ParameterSet.Parse guarantees ArgIndexKind is available
            Debug.Assert(index.Kind != ArgIndexKind.Flag);
            return index;
        }
        else {
            return null;
        }
    }

    private bool GetRawFlag(string key)
    {
        if (_optionDict.TryGetValue(key, out var index)) {
            Debug.Assert(index.Kind == ArgIndexKind.Flag);
            return true;
        }
        else {
            return false;
        }
    }

    private ReadOnlySpan<ArgIndex> GetRawValuesIndices(int index, int count)
    {
        if (index < _valueList.Length) {
            var end = index + count;
            Debug.Assert(end <= _valueList.Length);
            return _valueList[index..end];
        }
        else {
            return default;
        }
    }

    /// <summary>
    /// Get the available length of Values parameter,
    /// this method is for stackalloc unmanaged type span
    /// </summary>
    public int GetAvailableArrayLength(int startIndex, int exceptedLength)
    {
        var length = _valueList.Length;
        if (startIndex >= length)
            return 0;
        return int.Min(exceptedLength, length - startIndex);
    }
}
