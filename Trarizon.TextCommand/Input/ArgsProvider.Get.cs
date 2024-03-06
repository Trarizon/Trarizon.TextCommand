using System.Runtime.InteropServices;
using Trarizon.TextCommand.Input.Result;
using Trarizon.TextCommand.Parsers;
using Trarizon.TextCommand.Attributes.Parameters;

namespace Trarizon.TextCommand.Input;
partial struct ArgsProvider
{
    /// <summary>
    /// Get option argument
    /// </summary>
    /// <typeparam name="T">Type that parser returns</typeparam>
    /// <typeparam name="TParser">Parser</typeparam>
    /// <param name="key">The option name, should match <see cref="OptionAttribute.Name"/></param>
    /// <param name="parser">Parser</param>
    /// <returns>A <see cref="ArgResult{T}"/> contains result</returns>
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

    /// <summary>
    /// Get flag argument
    /// </summary>
    /// <typeparam name="T">Type that parser returns</typeparam>
    /// <typeparam name="TParser">Parser</typeparam>
    /// <param name="key">The flag name, should match <see cref="FlagAttribute.Name"/></param>
    /// <param name="parser">Parser</param>
    /// <returns>The result value</returns>
    public T GetFlag<T, TParser>(string key, TParser parser) where TParser : IArgFlagParser<T>
    {
        return parser.Parse(GetRawFlag(key));
    }

    /// <summary>
    /// Get multi-value argument by stackalloc for unmanaged span
    /// </summary>
    /// <typeparam name="T">Type that parser returns</typeparam>
    /// <typeparam name="TParser">Parser</typeparam>
    /// <param name="startIndex">the start index in all value arguments</param>
    /// <param name="parser">Parser</param>
    /// <param name="allocatedSpace">Ahead-alloc space for result</param>
    /// <returns>A <see cref="ArgResultsUnmanaged{T}"/> contains result</returns>
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

    /// <summary>
    /// Get multi-value argument
    /// </summary>
    /// <typeparam name="T">Type that parser returns</typeparam>
    /// <typeparam name="TParser">Parser</typeparam>
    /// <param name="startIndex">the start index in all value arguments</param>
    /// <param name="parser">Parser</param>
    /// <param name="allocatedSpace">Ahead-alloc space for extra result infos</param>
    /// <returns>A <see cref="ArgResultsArray{T}"/> contains result</returns>
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

    /// <summary>
    /// Get multi-value argument
    /// </summary>
    /// <typeparam name="T">Type that parser returns</typeparam>
    /// <typeparam name="TParser">Parser</typeparam>
    /// <param name="startIndex">the start index in all value arguments</param>
    /// <param name="parser">Parser</param>
    /// <param name="allocatedSpace">Ahead-alloc space for extra result infos</param>
    /// <returns>A <see cref="ArgResultsList{T}"/> contains result</returns>
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

    /// <summary>
    /// Get value argument
    /// </summary>
    /// <typeparam name="T">Type that parser returns</typeparam>
    /// <typeparam name="TParser">Parser</typeparam>
    /// <param name="index">the start index in all value arguments</param>
    /// <param name="parser">Parser</param>
    /// <returns>A <see cref="ArgResult{T}"/> contains result</returns>
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
