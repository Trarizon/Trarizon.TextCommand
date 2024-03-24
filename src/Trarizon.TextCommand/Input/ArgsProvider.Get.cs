using System.Runtime.InteropServices;
using Trarizon.TextCommand.Input.Result;
using Trarizon.TextCommand.Parsers;
using Trarizon.TextCommand.Attributes.Parameters;
using System.ComponentModel;

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
    [EditorBrowsable(EditorBrowsableState.Never)]
    public ArgResult<T> GetOption<T, TParser>(string key, TParser parser) where TParser : IArgParser<T>
    {
        // Not set
        if (!TryGetRawOption(key, out var index))
            return ArgResult<T>.ParameterNotSet();

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
    [EditorBrowsable(EditorBrowsableState.Never)]
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
    /// <param name="allocatedSpace">
    /// Ahead-alloc space for result,
    /// size of this parameter is equals to the length of returned span
    /// </param>
    /// <returns>A <see cref="ArgResultsUnmanaged{T}"/> contains result</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public ArgResultsUnmanaged<T> GetValuesUnmanaged<T, TParser>(int startIndex, TParser parser, Span<ArgResult<T>> allocatedSpace) where TParser : IArgParser<T>
    {
        if (allocatedSpace.Length == 0)
            return ArgResultsUnmanaged<T>.Empty;

        var indices = GetRawValuesIndices(startIndex, allocatedSpace.Length);

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
    /// <param name="allocatedSpace">
    /// Ahead-alloc space for extra result infos,
    /// size of this parameter is equals to the length of returned array
    /// </param>
    /// <returns>A <see cref="ArgResultsArray{T}"/> contains result</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public ArgResultsArray<T> GetValuesArray<T, TParser>(int startIndex, TParser parser, Span<ArgRawResultInfo> allocatedSpace) where TParser : IArgParser<T>
    {
        if (allocatedSpace.Length == 0)
            return ArgResultsArray<T>.Empty;

        var indices = GetRawValuesIndices(startIndex, allocatedSpace.Length);

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
    /// <param name="allocatedSpace">
    /// Ahead-alloc space for extra result infos,
    /// size of this parameter is equals to the length of returned list
    /// </param>
    /// <returns>A <see cref="ArgResultsList{T}"/> contains result</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public ArgResultsList<T> GetValuesList<T, TParser>(int startIndex, TParser parser, Span<ArgRawResultInfo> allocatedSpace) where TParser : IArgParser<T>
    {
        if (allocatedSpace.Length > 0)
            return ArgResultsList<T>.Empty;

        var indices = GetRawValuesIndices(startIndex, allocatedSpace.Length);

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
    [EditorBrowsable(EditorBrowsableState.Never)]
    public ArgResult<T> GetValue<T, TParser>(int index, TParser parser) where TParser : IArgParser<T>
    {
        // Not set
        if (!TryGetRawValueIndex(index, out var argIndex))
            return ArgResult<T>.ParameterNotSet();

        // Parsing failed
        if (!TryParseArg<T, TParser>(argIndex, parser, out var result))
            return ArgResult<T>.ParsingFailed(argIndex);

        // Success
        return ArgResult<T>.NoError(result);
    }
}
