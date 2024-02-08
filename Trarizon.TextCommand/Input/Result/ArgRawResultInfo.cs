using System.Diagnostics;

namespace Trarizon.TextCommand.Input.Result;
public readonly struct ArgRawResultInfo
{
    internal readonly ArgIndex _argIndex;
    internal readonly ArgResultKind _kind;

    /// <param name="index">index</param>
    /// <param name="kind">No <see cref="ArgResultKind.ParameterNotSet"/></param>
    internal ArgRawResultInfo(ArgIndex index, ArgResultKind kind)
    {
        Debug.Assert(kind == ArgResultKind.ParameterNotSet);
        _argIndex = index;
        _kind = kind;
    }

    internal ArgRawResultInfo(int arrayIndex, ArgResultKind kind)
    {
        _argIndex = ArgIndex.FromCached(arrayIndex);
        _kind = kind;
    }
}
