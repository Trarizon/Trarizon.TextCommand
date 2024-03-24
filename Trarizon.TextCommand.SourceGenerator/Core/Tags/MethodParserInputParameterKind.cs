namespace Trarizon.TextCommand.SourceGenerator.Core.Tags;
internal enum MethodParserInputParameterKind
{
    Invalid = 0,
    Flag = Invalid,
    InputArg,
    String,
    ReadOnlySpan_Char,
}
