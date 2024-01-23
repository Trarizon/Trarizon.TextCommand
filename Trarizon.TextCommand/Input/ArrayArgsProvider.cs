using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Trarizon.TextCommand.Parsers;

namespace Trarizon.TextCommand.Input;
[EditorBrowsable(EditorBrowsableState.Never)]
public readonly ref partial struct ArrayArgsProvider
{
    private readonly Dictionary<string, string?> _dict;
    private readonly ReadOnlySpan<string> _list;

    internal ArrayArgsProvider(Dictionary<string, string?> dict, ReadOnlySpan<string> list)
    {
        _dict = dict;
        _list = list;
    }

    private static bool TryParseArg<T, TParser>(string rawArg, TParser parser, [MaybeNullWhen(false)] out T result) where TParser : IArgParser<T>
    {
        if (typeof(TParser) == typeof(ParsableParser<string>)) {
            result = Unsafe.As<string, T>(ref rawArg);
            return true;
        }
        else {
            return parser.TryParse(rawArg, out result);
        }
    }

    /// <returns>Returns the index of first error</returns>
    private static int TryParseArgs<T, TParser>(ReadOnlySpan<string> rawArgs, TParser parser, Span<T> resultSpan) where TParser : IArgParser<T>
    {
        for (int i = 0; i < rawArgs.Length; i++) {
            if (!TryParseArg(rawArgs[i], parser, out resultSpan[i]!))
                return i;
        }
        return -1;
    }

    private bool TryGetRawOption(string key, [NotNullWhen(true)] out string? optionArg)
    {
        if (_dict.TryGetValue(key, out optionArg!)) {
            Debug.Assert(optionArg != null);
            return true;
        }
        else {
            optionArg = default;
            return false;
        }
    }

    private bool GetRawFlag(string key)
    {
        var res = _dict.TryGetValue(key, out var val);
        Debug.Assert(val is null);
        return res;
    }

    private bool TryGetRawValues(int index, int count, out ReadOnlySpan<string> values)
    {
        if (index < _list.Length) {
            var end = int.Min(index + count, _list.Length);
            values = _list[index..end];
            return true;
        }
        else {
            values = default;
            return false;
        }
    }

    public int GetAvailableArrayLength(int startIndex, int expectedLength)
        => int.Min(expectedLength, _list.Length - startIndex);
}
