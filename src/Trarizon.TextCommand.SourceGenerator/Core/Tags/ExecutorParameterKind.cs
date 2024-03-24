namespace Trarizon.TextCommand.SourceGenerator.Core.Tags;
internal enum ExecutorParameterKind
{
    Invalid = 0,
    Implicit = Invalid,
    Flag,
    Option,
    Value,
    MultiValue,
    Context,
}
