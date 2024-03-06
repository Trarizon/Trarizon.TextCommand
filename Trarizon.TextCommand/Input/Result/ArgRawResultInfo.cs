using System.Diagnostics;

namespace Trarizon.TextCommand.Input.Result;
public readonly struct ArgRawResultInfo
{
    internal readonly ArgIndex _argIndex;
    internal readonly ArgResultKind _kind;

    internal ArgRawResultInfo(ArgIndex index, ArgResultKind kind)
    {
        _argIndex = index;
        _kind = kind;
    }

    internal ArgRawResultInfo(int arrayIndex, ArgResultKind kind)
    {
        _argIndex = ArgIndex.FromCached(arrayIndex);
        _kind = kind;
    }
}
