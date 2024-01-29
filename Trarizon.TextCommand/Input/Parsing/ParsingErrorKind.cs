namespace Trarizon.TextCommand.Input.Parsing;
// This also means error level, do not change the order of members

/// <summary>
/// Error kind
/// </summary>
public enum ParsingErrorKind
{
    NoError = 0,
    ParameterNotSet,
    ParsingFailed,
}
