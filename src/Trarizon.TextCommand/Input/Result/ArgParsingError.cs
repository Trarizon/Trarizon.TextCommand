namespace Trarizon.TextCommand.Input.Result;
internal readonly struct ArgParsingError(ArgRawResultInfo rawInfo, Type resultType, string parameterName)
{
    internal readonly ArgRawResultInfo _rawInfo = rawInfo;
    internal readonly Type _resultType = resultType;
    internal readonly string _parameterName = parameterName;
}
