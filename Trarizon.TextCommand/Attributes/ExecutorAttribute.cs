namespace Trarizon.TextCommand.Attributes;
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class ExecutorAttribute(params string[] commandPrefixes) : Attribute
{
    public string[] CommandPrefixes => commandPrefixes;
}
