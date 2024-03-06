namespace Trarizon.TextCommand.Input.Result;
internal readonly struct ArgParsingError(ArgRawResultInfo rawInfo, Type parsedType, string parameterName)
{
    internal readonly ArgRawResultInfo _rawInfo = rawInfo;
    internal readonly Type _parsedType = parsedType;
    internal readonly string _parameterName = parameterName;
}
