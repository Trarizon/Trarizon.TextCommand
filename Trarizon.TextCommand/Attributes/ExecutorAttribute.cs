namespace Trarizon.TextCommand.Attributes;
[AttributeUsage(AttributeTargets.Method)]
public sealed class ExecutorAttribute(params string[] commandPrefixes) : Attribute
{
    public string[] CommandPrefixes => commandPrefixes;
}
