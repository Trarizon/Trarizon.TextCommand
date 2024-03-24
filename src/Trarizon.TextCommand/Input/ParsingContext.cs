using System.Collections.Frozen;
using System.ComponentModel;
using System.Diagnostics;
using Trarizon.TextCommand.Utilities;

namespace Trarizon.TextCommand.Input;
/// <summary>
/// The parameter set helper for parse raw input into typed args
/// </summary>
/// <param name="name_ParamCountDict">
/// name-paramCount dictionary, contains all named parameters;<br/>
/// key is command parameter name, value is the extra arg count the parameter requires(flag 0, option 1)
/// </param>
/// <param name="aliasDict"></param>
public sealed class ParsingContext(
    // 数字表示该值需要的参数数量，目前只需要1和0，
    // 1表示option，0表示flag
    Dictionary<string, int>? name_ParamCountDict,
    Dictionary<string, string>? aliasDict)
{
    private readonly FrozenDictionary<string, int> _name_ParamCountDict = name_ParamCountDict?.ToFrozenDictionary() ?? FrozenDictionary<string, int>.Empty;
    private readonly FrozenDictionary<string, string> _aliasDict = aliasDict?.ToFrozenDictionary() ?? FrozenDictionary<string, string>.Empty;

    /// <summary>
    /// Parse string
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public ArgsProvider Parse(in StringInputRest rest)
    {
        Dictionary<string, ArgIndex> dict = [];
        // slice
        // from cache
        // flag
        AllocOptList<ArgIndex> list = new();
        string[] unescapeds = new string[rest.CountOfEscapes];
        int unescapeCount = 0;

        for (int i = 0; i < rest.Indices.Length; i++) {
            var index = rest.Indices[i];

            // Escaped
            if (index.Kind == ArgIndexKind.Escaped) {
                var (start, length) = index.EscapedRange;
                var unescaped = StringUtil.UnescapeToString(rest.Source.Slice(start, length));

                list.Add(ArgIndex.FromCached(unescapeCount));
                unescapeds[unescapeCount++] = unescaped;
                continue;
            }
            // Slice
            else {
                var (start, length) = index.SliceRange;
                var arg = rest.Source.Slice(start, length);

                string? strArg;
                switch (arg) {
                    case ['-', '-', '-', ..]:
                        goto default;
                    case ['-', '-', .. var nameKey]:
                        strArg = nameKey.ToString();
                        break;
                    case ['-', .. var aliasKey]:
                        // Not defined option key, omit
                        if (!_aliasDict.TryGetValue(aliasKey.ToString(), out strArg))
                            continue;
                        break;
                    // Value | MultiValue
                    default:
                        list.Add(index);
                        continue;
                }

                if (_name_ParamCountDict.TryGetValue(strArg, out var isOption)) {
                    // Flag
                    if (isOption == 0) {
                        dict.Add(strArg, ArgIndex.Flag);
                        continue;
                    }

                    // Option
                    if (++i < rest.Indices.Length) {
                        var nextIndex = rest.Indices[i];
                        if (nextIndex.Kind == ArgIndexKind.Escaped) {
                            (start, length) = nextIndex.EscapedRange;
                            var unescaped = StringUtil.UnescapeToString(rest.Source.Slice(start, length));
                            dict.Add(strArg, ArgIndex.FromCached(unescapeCount));
                            unescapeds[unescapeCount++] = unescaped;
                        }
                        else {
                            dict.Add(strArg, rest.Indices[i]);
                        }
                    }

                    // else the option key is the last value in input, omit
                }
                // Not defined option key, omit it
                else { }
            }
        }

        Debug.Assert(unescapeCount == unescapeds.Length);

        return new(rest.Source, unescapeds, dict, list.AsSpan());
    }

    /// <summary>
    /// Parse string span
    /// </summary>
    public ArgsProvider Parse(ReadOnlySpan<string> args)
    {
        Dictionary<string, ArgIndex> dict = [];
        AllocOptList<ArgIndex> list = new();

        for (int i = 0; i < args.Length; i++) {
            var arg = args[i];
            switch (arg) {
                case ['-', '-', '-', ..]:
                    goto default;
                case ['-', '-', .. var nameKey]:
                    arg = nameKey;
                    break;
                case ['-', .. var aliasKey]:
                    // Not defined option key, omit
                    if (!_aliasDict.TryGetValue(aliasKey, out arg))
                        continue;
                    break;
                default:
                    list.Add(ArgIndex.FromCached(i));
                    continue;
            }

            if (_name_ParamCountDict.TryGetValue(arg, out var isOption)) {
                // Flag
                if (isOption == 0) {
                    dict.Add(arg, ArgIndex.Flag);
                    continue;
                }

                // Option
                if (++i < args.Length) {
                    dict.Add(arg, ArgIndex.FromCached(i));
                }

                // else option key is the last arg of input
            }
            // Not defined option key, omit
            else { }
        }

        return new ArgsProvider(default, args, dict, list.AsSpan());
    }

    /// <summary>
    /// Parse string collection
    /// </summary>
    public ArgsProvider Parse<TEnumerable>(TEnumerable args) where TEnumerable : IEnumerable<string>
    {
        Dictionary<string, ArgIndex> dict = [];
        AllocOptList<ArgIndex> indexList = new();

        using var enumerator = EnumerableExtensions.GetIndexedEnumerator<TEnumerable, string>(args);
        while (enumerator.TryMoveNext(out var index, out var arg)) {
            switch (arg) {
                case ['-', '-', '-', ..]:
                    goto default;
                case ['-', '-', .. var nameKey]:
                    arg = nameKey;
                    break;
                case ['-', .. var aliasKey]:
                    // Not defined option key, omit
                    if (!_aliasDict.TryGetValue(aliasKey, out arg))
                        continue;
                    break;
                // Value | MultiValue
                default:
                    indexList.Add(ArgIndex.FromCached(index));
                    break;
            }

            if (_name_ParamCountDict.TryGetValue(arg, out var isOption)) {
                // Flag
                if (isOption == 0) {
                    dict.Add(arg, ArgIndex.Flag);
                    continue;
                }

                // Option
                if (enumerator.TryMoveNext(out var nextIndex, out _)) {
                    dict.Add(arg, ArgIndex.FromCached(nextIndex));
                }
                // else the option key is the last value in input, omit
            }
            // Not defined option key, omit
            else { }
        }

        AllocOptList<string> inputArgs = new(enumerator.IteratedCount);
        foreach (var arg in args)
            inputArgs.Add(arg);

        return new ArgsProvider(default, inputArgs.AsSpan(), dict, indexList.AsSpan());
    }
}
