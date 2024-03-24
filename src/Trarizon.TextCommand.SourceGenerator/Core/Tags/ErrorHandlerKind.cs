namespace Trarizon.TextCommand.SourceGenerator.Core.Tags;
internal enum ErrorHandlerKind
{
    // As proirity
    Invalid=0,
    /// <summary>
    /// (Errors) -> TRtn
    /// </summary>
    Minimal,
    /// <summary>
    /// (Errors,string) -> TRtn
    /// </summary>
    WithExecutorName,
}
