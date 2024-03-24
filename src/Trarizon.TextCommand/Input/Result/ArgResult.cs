using System.ComponentModel;

namespace Trarizon.TextCommand.Input.Result;
/// <summary>
/// Parsing result contains single Value
/// </summary>
/// <typeparam name="T">Type of the value</typeparam>
[EditorBrowsable(EditorBrowsableState.Never)]
public readonly struct ArgResult<T>
{
    private readonly T? _value;
    internal readonly ArgRawResultInfo _rawResultInfo;

    /// <summary>
    /// Has value when <see cref="_rawResultInfo"/> is <see cref="ArgResultKind.NoError"/>
    /// </summary>
    public T Value => _value!;

    private ArgResult(T? value, in ArgRawResultInfo resultInfo)
    {
        _value = value;
        _rawResultInfo = resultInfo;
    }

    internal static ArgResult<T> NoError(T value) => new(value, new(default(ArgIndex), ArgResultKind.NoError));

    internal static ArgResult<T> ParsingFailed(ArgIndex index) => new(default, new(index, ArgResultKind.ParsingFailed));
    internal static ArgResult<T> ParsingFailed(int index) => new(default, new(index, ArgResultKind.ParsingFailed));

    internal static ArgResult<T> ParameterNotSet() => new(default, new(default(ArgIndex), ArgResultKind.ParameterNotSet));
}
