namespace Trarizon.TextCommand.Attributes;
/// <summary>
/// Execution entrance of the command
/// </summary>
/// <param name="commandName">Command prefix</param>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ExecutionAttribute(string? commandName = null) : Attribute
{
    /// <summary>
    /// Command prefix
    /// </summary>
    public string? CommandName => commandName;

    /// <summary>
    /// Member method name of custom error handler
    /// </summary>
    public string? ErrorHandler { get; init; }
}
