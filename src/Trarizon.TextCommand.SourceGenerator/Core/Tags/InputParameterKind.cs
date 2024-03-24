namespace Trarizon.TextCommand.SourceGenerator.Core.Tags;
internal enum InputParameterKind
{
    Invalid = 0,
    String,
    ReadOnlySpan_Char = String,
    CustomMatcher,
}
