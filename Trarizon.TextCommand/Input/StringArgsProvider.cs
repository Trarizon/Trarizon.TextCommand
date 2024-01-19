using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Trarizon.TextCommand.Parsers;

namespace Trarizon.TextCommand.Input;
[EditorBrowsable(EditorBrowsableState.Never)]
public readonly ref struct StringArgsProvider
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

    private T GetArg<T, TParser>(ArgIndex index, TParser parser) where TParser : IArgParser<T>
    {
        // Slice
        if (index.Kind == ArgIndexKind.Slice) {
            var (start, length) = index.SliceRange;
            var rawSpan = _sourceInput.Slice(start, length);
            if (parser.TryParse(rawSpan, out var result))
                return result;
            else
                throw new FormatException($"Cannot parse '{rawSpan}' to {typeof(T)}");
        }
        // Specialized for string,
        // to avoid string->ROS<char>->string conversion while parse
        else if (typeof(TParser) == typeof(ParsableParser<string>)) {
            var rawArg = _unescapeds[index.CachedIndex];
            return Unsafe.As<string, T>(ref rawArg);
        }
        // from cached
        else {
            var rawArg = _unescapeds[index.CachedIndex];
            if (parser.TryParse(rawArg, out var result))
                return result;
            else
                throw new FormatException($"Cannot parse '{rawArg}' to {typeof(T)}");

        }
    }

    private bool TryGetOption(string key, bool mayThrow, out ArgIndex argIndex)
    {
        if (_dict.TryGetValue(key, out var index)) {
            if (index.Kind != ArgIndexKind.Flag) {
                argIndex = index;
                return true;
            }
            else {
                throw new ArgumentException($"Option '{key}' does not exist,", nameof(key));
            }
        }
        else if (mayThrow)
            throw new ArgumentException($"Argument of '{key}' does not exist.", nameof(key));
        else {
            argIndex = default;
            return false;
        }
    }

    private bool GetFlag(string key)
        => _dict.TryGetValue(key, out var index) && index.Kind == ArgIndexKind.Flag;

    private bool TryGetValueIndexes(int index, int count, string? keyName, out ReadOnlySpan<ArgIndex> indexes)
    {
        if (index < _list.Length) {
            var end = int.Min(index + count, _list.Length);
            indexes = _list[index..end];
            return true;
        }
        else if (keyName is null) {
            indexes = default;
            return false;
        }
        else {
            throw new ArgumentException($"Parameter `{keyName}` has no value");
        }
    }

    public T? GetOption<T, TParser>(string key, TParser parser, bool throwIfNotExist) where TParser : IArgParser<T>
    {
        if (TryGetOption(key, throwIfNotExist, out var argIndex)) {
            return GetArg<T, TParser>(argIndex, parser);
        }
        return default;
    }

    public T GetFlag<T, TParser>(string key, TParser parser) where TParser : IArgFlagParser<T>
    {
        return parser.Parse(GetFlag(key));
    }

    public Span<T> GetValues<T, TParser>(int startIndex, Span<T> resultSpan, TParser parser, string? keyAsThrowFlag) where TParser : IArgParser<T>
    {
        if (TryGetValueIndexes(startIndex, resultSpan.Length, keyAsThrowFlag, out var values)) {
            for (int i = 0; i < resultSpan.Length; i++) {
                resultSpan[i] = GetArg<T, TParser>(values[i], parser);
            }
            return resultSpan;
        }
        return default;
    }

    public T[] GetRestValues<T, TParser>(int startIndex, TParser parser, string? keyAsThrowFlag) where TParser : IArgParser<T>
    {
        T[] array = new T[_list.Length - startIndex];
        GetValues<T, TParser>(startIndex, array, parser, keyAsThrowFlag);
        return array;
    }

    public List<T> GetRestValuesList<T, TParser>(int startIndex, TParser parser, string? keyAsThrowFlag) where TParser : IArgParser<T>
    {
        var list = new List<T>(_list.Length - startIndex);
        GetValues<T, TParser>(startIndex, CollectionsMarshal.AsSpan(list), parser, keyAsThrowFlag);
        return list;
    }

    public T[] GetValuesArray<T, TParser>(int startIndex, int length, TParser parser, string? keyAsThrowFlag) where TParser : IArgParser<T>
    {
        T[] array = new T[length];
        GetValues<T, TParser>(startIndex, array, parser, keyAsThrowFlag);
        return array;
    }

    public List<T> GetValuesList<T, TParser>(int startIndex, int length, TParser parser, string? keyAsThrowFlag) where TParser : IArgParser<T>
    {
        List<T> list = new(length);
        GetValues(startIndex, CollectionsMarshal.AsSpan(list), parser, keyAsThrowFlag);
        return list;
    }

    public T? GetValue<T, TParser>(int index, TParser parser, string? keyAndThrowFlag) where TParser : IArgParser<T>
    {
        T val = default!;
        GetValues(index, MemoryMarshal.CreateSpan(ref val, 1), parser, keyAndThrowFlag);
        return val;
    }
}
