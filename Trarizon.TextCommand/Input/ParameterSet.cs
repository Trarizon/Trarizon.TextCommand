using System.Collections.Frozen;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Trarizon.TextCommand.Utilities;

namespace Trarizon.TextCommand.Input;
public sealed class ParameterSet(
    Dictionary<string, bool>? optionOrFlagParameters,
    Dictionary<string, string>? aliasDict)
{
    private readonly FrozenDictionary<string, bool> _optionOrFlagParameters = optionOrFlagParameters?.ToFrozenDictionary() ?? FrozenDictionary<string, bool>.Empty;
    private readonly FrozenDictionary<string, string> _aliasDict = aliasDict?.ToFrozenDictionary() ?? FrozenDictionary<string, string>.Empty;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public StringArgsProvider Parse(in StringInputRest rest)
    {
        Dictionary<string, ArgIndex> dict = [];
        // slice
        // from cache
        // flag
        List<ArgIndex> list = [];
        string[] unescapeds = new string[rest.CountOfEscapes];
        int unescapeCount = 0;

        for (int i = 0; i < rest.Indexes.Length; i++) {
            var index = rest.Indexes[i];

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

                string strArg;
                switch (arg) {
                    case ['-', '-', ..]:
                        strArg = arg.ToString();
                        break;
                    case ['-', ..]:
                        // Not defined option key, omit
                        if (!_aliasDict.TryGetValue(arg.ToString(), out strArg!))
                            continue;
                        break;
                    // Value | MultiValue
                    default:
                        list.Add(index);
                        continue;
                }

                if (_optionOrFlagParameters.TryGetValue(strArg, out var isOption)) {
                    // Flag
                    if (!isOption) {
                        dict.Add(strArg, ArgIndex.Flag);
                        continue;
                    }

                    // Option
                    if (++i < rest.Indexes.Length) {
                        dict.Add(strArg, rest.Indexes[i]);
                        continue;
                    }

                    // else the option key is the last value in input, omit
                }
                // Not defined option key, omit it
                else { }
            }
        }

        return new(rest.Source, unescapeds, dict, CollectionsMarshal.AsSpan(list));
    }

    public ArrayArgsProvider Parse(ReadOnlySpan<string> args)
    {
        Dictionary<string, string?> dict = [];
        List<string> list = [];

        for (int i = 0; i < args.Length; i++) {
            var arg = args[i];
            switch (arg) {
                case ['-', '-', ..]:
                    break;
                case ['-', ..]:
                    // Not defined option key, omit
                    if (!_aliasDict.TryGetValue(arg, out arg))
                        continue;
                    break;
                default:
                    list.Add(arg);
                    break;
            }

            if (_optionOrFlagParameters.TryGetValue(arg, out var isOption)) {
                if (!isOption)
                    dict.Add(arg, null);
                else if (++i < args.Length)
                    dict.Add(arg, args[i]);
                // else option key is the last arg of input
                break;
            }
            // Not defined option key, omit
            else { }
        }

        return new ArrayArgsProvider(dict, CollectionsMarshal.AsSpan(list));
    }

    /* TODO: does input support other collections like IList<>?
    
    public ArrayArgsProvider Parse(IEnumerable<string> args)
    {
        Dictionary<string, string?> dict = [];
        List<string> list = [];

        using var enumerator = args.GetEnumerator();

        while (enumerator.MoveNext()) {
            var arg = enumerator.Current;
            switch (arg) {
                case ['-', '-', ..]:
                    if (_optionOrFlagParameters.TryGetValue(arg, out var isOption)) {
                        if (!isOption)
                            dict.Add(arg, null);
                        else if (enumerator.MoveNext())
                            dict.Add(arg, enumerator.Current);
                        // else option key is the last arg of input
                        break;
                    }
                    if (ThrowIfOnRedundantArgument) {
                        throw new Exception();
                    }
                    break;
                case ['-', ..]:
                    if (_aliasDict.TryGetValue(arg, out arg) && _optionOrFlagParameters.TryGetValue(arg, out isOption)) {
                        if (!isOption)
                            dict.Add(arg, null);
                        else if (enumerator.MoveNext())
                            dict.Add(arg, enumerator.Current);
                        // else option key is the last arg of input
                        break;
                    }
                    else if (ThrowIfOnRedundantArgument) {
                        throw new Exception();
                    }
                    break;
                default:
                    list.Add(arg);
                    break;
            }
        }

        return new ArrayArgsProvider(dict, CollectionsMarshal.AsSpan(list));
    }

    */
}
