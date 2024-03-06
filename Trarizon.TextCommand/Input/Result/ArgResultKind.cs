namespace Trarizon.TextCommand.Input.Result;
// This also means error level, do not change the order of members

/// <summary>
/// Error kind
/// </summary>
public enum ArgResultKind
{
    /// <summary>
    /// Parsig succeed
    /// </summary>
    NoError = 0,
    /// <summary>
    /// Parameter is not set
    /// </summary>
    ParameterNotSet = NoError + 1,
    /// <summary>
    /// Failed to parsing input to a specific type
    /// </summary>
    ParsingFailed = ParameterNotSet + 1,
}
