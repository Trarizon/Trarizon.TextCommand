using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Trarizon.TextCommand.Parsers;

namespace Trarizon.TextCommand.Input;
public readonly ref struct ArrayArgsProvider
{
    private readonly Dictionary<string, string?> _dict;
    private readonly ReadOnlySpan<string> _list;

    internal ArrayArgsProvider(Dictionary<string, string?> dict, ReadOnlySpan<string> list)
    {
        _dict = dict;
        _list = list;
    }

    private T GetArg<T, TParser>(string rawArg, TParser parser) where TParser : IArgParser<T>
    {
        // Specialized for string,
        if (typeof(TParser) == typeof(ParsableParser<string>)) {
            return Unsafe.As<string, T>(ref rawArg);
        }
        else if (parser.TryParse(rawArg, out var result))
            return result;
        else
            throw new FormatException($"Cannot parse `{rawArg}` to {typeof(T).Name}");
    }

    private bool TryGetOption(string key, bool mayThrow, [NotNullWhen(true)] out string? optionArg)
    {
        if (_dict.TryGetValue(key, out optionArg!)) {
            Debug.Assert(optionArg != null);
            return true;
        }
        else if (mayThrow)
            throw new ArgumentException($"Argument of '{key}' does not exist.", nameof(key));
        else {
            optionArg = default;
            return false;
        }
    }

    private bool GetFlag(string key)
    {
        var res = _dict.TryGetValue(key, out var val);
        Debug.Assert(val is null);
        return res;
    }

    private bool TryGetValueIndexes(int index, int count, string? keyName, out ReadOnlySpan<string> values)
    {
        if (index < _list.Length) {
            var end = int.Min(index + count, _list.Length);
            values = _list[index..end];
            return true;
        }
        else if (keyName is null) {
            values = default;
            return false;
        }
        else {
            throw new ArgumentException($"Parameter `{keyName}` has no value");
        }
    }

    public T? GetOption<T, TParser>(string key, TParser parser, bool throwIfNotExist) where TParser : IArgParser<T>
    {
        if (TryGetOption(key, throwIfNotExist, out var argument)) {
            return GetArg<T, TParser>(argument, parser);
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
