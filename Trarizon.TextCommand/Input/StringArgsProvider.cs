using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.TextCommand.Input.Parsing;
using Trarizon.TextCommand.Parsers;

namespace Trarizon.TextCommand.Input;
/// <summary>
/// Parse and get arguments from input
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public readonly ref partial struct StringArgsProvider
{
	private readonly ReadOnlySpan<char> _sourceInput;
	private readonly ReadOnlySpan<string> _unescapeds;

	// slice
	// from cached
	// flag
	private readonly Dictionary<string, ArgIndex> _dict;
	private readonly ReadOnlySpan<ArgIndex> _list;

	internal StringArgsProvider(ReadOnlySpan<char> sourceInput, ReadOnlySpan<string> unescapeds, Dictionary<string, ArgIndex> dict, ReadOnlySpan<ArgIndex> list)
	{
		_sourceInput = sourceInput;
		_unescapeds = unescapeds;
		_dict = dict;
		_list = list;
	}

	private bool TryParseArg<T, TParser>(ArgIndex index, TParser parser, [MaybeNullWhen(false)] out T result, out ReadOnlySpan<char> rawSpan) where TParser : IArgParser<T>
	{
		// Slice
		if (index.Kind == ArgIndexKind.Slice) {
			var (start, length) = index.SliceRange;
			rawSpan = _sourceInput.Slice(start, length);
			return parser.TryParse(rawSpan, out result);
		}
		// From cached
		else {
			var rawArg = _unescapeds[index.CachedIndex];
			rawSpan = rawArg;

			// Specialized for string,
			// to avoid string->ROS<char>->string conversion while parse
			if (typeof(TParser) == typeof(ParsableParser<string>)) {
				result = Unsafe.As<string, T>(ref rawArg);
				return true;
			}
			else {
				return parser.TryParse(rawSpan, out result);
			}
		}
	}

	/// <returns>Returns the index of first error</returns>
	private int TryParseArgs<T, TParser>(ReadOnlySpan<ArgIndex> indices, TParser parser, Span<T> resultSpan) where TParser : IArgParser<T>
	{
		for (int i = 0; i < indices.Length; i++) {
			if (!TryParseArg(indices[i], parser, out resultSpan[i]!, out _))
				return i;
		}
		return -1;
	}

    private void TryParseArgs<T, TParser>(ReadOnlySpan<ArgIndex> indices, TParser parser, Span<T> resultsSpan, Span<ParsingErrorSimple> errorSpan) where TParser : IArgParser<T>
    {
        for (int i = 0; i < indices.Length; i++) {
            var argIndex = indices[i];
            if (!TryParseArg(argIndex, parser, out resultsSpan[i]!)) {
                errorSpan[i] = new(argIndex);
            }
        }
    }

    private bool TryGetRawOptionIndex(string key, out ArgIndex argIndex)
    {
        if (_dict.TryGetValue(key, out var index)) {
            // ParameterSet.Parse guarantees ArgIndexKind is available
            Debug.Assert(index.Kind != ArgIndexKind.Flag);

			argIndex = index;
			return true;
		}
		else {
			argIndex = default;
			return false;
		}
	}

	private bool GetRawFlag(string key)
	{
		if (_dict.TryGetValue(key, out var index)) {
			Debug.Assert(index.Kind == ArgIndexKind.Flag);
			return true;
		}
		else {
			return false;
		}
	}

    private bool TryGetRawValuesIndices(int index, int count, out ReadOnlySpan<ArgIndex> indices)
    {
        if (index < _list.Length) {
            var end = index + count;
            Debug.Assert(end <= _list.Length);
            indices = _list[index..end];
            return true;
        }
        else {
            indices = default;
            return false;
        }
    }

    public int GetAvailableArrayLength(int startIndex, int exceptedLength)
        => int.Min(exceptedLength, _list.Length - startIndex);

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal ParsingErrors GetErrors(string executorMethodName, ReadOnlySpan<ParsingError> errorInfos)
        => new(
            _sourceInput,
            _unescapeds,
            executorMethodName,
            errorInfos);
}
