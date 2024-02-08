using System.Runtime.InteropServices;
using Trarizon.TextCommand.Input.Result;
using Trarizon.TextCommand.Parsers;

namespace Trarizon.TextCommand.Input;
partial struct ArgsProvider
{
    public ArgResult<T> GetOption<T, TParser>(string key, TParser parser) where TParser : IArgParser<T>
    {
        var nullableIndex = GetRawOption(key);
        // Not set
        if (nullableIndex is null)
            return ArgResult<T>.ParameterNotSet();

        var index = nullableIndex.GetValueOrDefault();
        // Parsing failed
        if (!TryParseArg<T, TParser>(index, parser, out var result))
            return ArgResult<T>.ParsingFailed(index);

        // Success
        return ArgResult<T>.NoError(result);
    }

    public T GetFlag<T, TParser>(string key, TParser parser) where TParser : IArgFlagParser<T>
    {
        return parser.Parse(GetRawFlag(key));
    }

    public ArgResultsUnmanaged<T> GetValuesUnmanaged<T, TParser>(int startIndex, TParser parser, Span<ArgResult<T>> allocatedSpace) where T : unmanaged where TParser : IArgParser<T>
    {
        var indices = GetRawValuesIndices(startIndex, allocatedSpace.Length);
        // Not set
        if (indices.Length == 0)
            return default;

        var rtn = new ArgResultsUnmanaged<T>(allocatedSpace);
        ParseArgs(indices, parser, rtn.Values, rtn.RawInfos);

        return rtn;
    }

    public ArgResultsArray<T> GetValuesArray<T, TParser>(int startIndex, TParser parser, Span<ArgRawResultInfo> allocatedSpace) where TParser : IArgParser<T>
    {
        var indices = GetRawValuesIndices(startIndex, allocatedSpace.Length);
        // Not set
        if (indices.Length == 0)
            return default;

        var rtn = new ArgResultsArray<T>(allocatedSpace);
        ParseArgs<T, TParser>(indices, parser, rtn.Values, rtn.RawInfos);

        return rtn;
    }

    public ArgResultsList<T> GetValuesList<T, TParser>(int startIndex, TParser parser, Span<ArgRawResultInfo> allocatedSpace) where TParser : IArgParser<T>
    {
        var indices = GetRawValuesIndices(startIndex, allocatedSpace.Length);
        // Not set
        if (indices.Length == 0)
            return default;

        var rtn = new ArgResultsList<T>(allocatedSpace);
        ParseArgs(indices, parser, CollectionsMarshal.AsSpan(rtn.Values), rtn.RawInfos);

        return rtn;
    }

    public ArgResult<T> GetValue<T, TParser>(int index, TParser parser) where TParser : IArgParser<T>
    {
        var argIndices = GetRawValuesIndices(index, 1);
        // Not set
        if (argIndices.Length == 0)
            return ArgResult<T>.ParameterNotSet();

        var argIndex = argIndices[0];
        // Parsing failed
        if (!TryParseArg<T, TParser>(argIndex, parser, out var result))
            return ArgResult<T>.ParsingFailed(argIndex);

        // Success
        return ArgResult<T>.NoError(result);
    }
}
