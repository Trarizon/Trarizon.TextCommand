namespace Trarizon.TextCommand.Attributes;
[AttributeUsage(AttributeTargets.Method)]
public sealed class ExecutionAttribute(string? commandName = null) : Attribute
{
    public string? CommandName => commandName;

    public string? ErrorHandler { get; init; }
}
