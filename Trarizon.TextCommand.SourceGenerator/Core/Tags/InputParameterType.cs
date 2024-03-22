namespace Trarizon.TextCommand.SourceGenerator.Core.Tags;
internal enum InputParameterType
{
    Invalid = 0,
    String = 1,
    ReadOnlySpan_Char = String,
    CustomMatcher,
}
