namespace Trarizon.TextCommand.Input.Result;
// This also means error level, do not change the order of members

/// <summary>
/// Error kind
/// </summary>
public enum ArgResultKind
{
	NoError = 0,
	ParameterNotSet,
	ParsingFailed,
}
