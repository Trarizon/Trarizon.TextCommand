using System.Runtime.InteropServices;
using Trarizon.TextCommand.Input.Parsing;
using Trarizon.TextCommand.Parsers;

namespace Trarizon.TextCommand.Input;
partial struct StringArgsProvider
{
    public ParsingResult<T> GetOption<T, TParser>(string key, TParser parser) where TParser : IArgParser<T>
    {
        if (!TryGetRawOptionIndex(key, out var argIndex))
            return new(ParsingErrorSimple.NotSet);

        if (!TryParseArg<T, TParser>(argIndex, parser, out var result))
            return new(new ParsingErrorSimple(argIndex));

        return new(result);
    }

    public T GetFlag<T, TParser>(string key, TParser parser) where TParser : IArgFlagParser<T>
    {
        return parser.Parse(GetRawFlag(key));
    }

    public ParsingResultsUnmanaged<T> GetValuesUnmanaged<T, TParser>(int startIndex, Span<ParsingResult<T>> allocatedSpace, TParser parser) where T : unmanaged where TParser : IArgParser<T>
    {
        if (!TryGetRawValuesIndices(startIndex, allocatedSpace.Length, out var argIndices))
            return default;

        var rtn = new ParsingResultsUnmanaged<T>(allocatedSpace);
        TryParseArgs(argIndices, parser, rtn.Results, rtn.Errors);

        return rtn;
    }

    public ParsingResultsArray<T> GetValuesArray<T, TParser>(int startIndex, Span<ParsingErrorSimple> allocatedErrorSpace, TParser parser) where TParser : IArgParser<T>
    {
        if (!TryGetRawValuesIndices(startIndex, allocatedErrorSpace.Length, out var argIndices))
            return default;

        var rtn = new ParsingResultsArray<T>(allocatedErrorSpace);
        TryParseArgs(argIndices, parser, rtn.Results.AsSpan(), rtn.Errors);

        return rtn;
    }

    public ParsingResultsList<T> GetValuesList<T, TParser>(int startIndex, Span<ParsingErrorSimple> allocatedErrorSpace, TParser parser) where TParser : IArgParser<T>
    {
        if (!TryGetRawValuesIndices(startIndex, allocatedErrorSpace.Length, out var argIndices))
            return default;

        var rtn = new ParsingResultsList<T>(allocatedErrorSpace);
        TryParseArgs(argIndices, parser, CollectionsMarshal.AsSpan(rtn.Results), rtn.Errors);

        return rtn;
    }

    public ParsingResult<T> GetValue<T, TParser>(int index, TParser parser) where TParser : IArgParser<T>
    {
        if (!TryGetRawValuesIndices(index, 1, out var argIndices))
            return default;

        var argIndex = argIndices[0];
        if (!TryParseArg<T, TParser>(argIndex, parser, out var result))
            return new(new ParsingErrorSimple(argIndex));
         
        return new(result);
    }
}
