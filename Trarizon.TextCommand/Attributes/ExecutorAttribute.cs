namespace Trarizon.TextCommand.Attributes;
/// <summary>
/// Executor 
/// </summary>
/// <param name="commandPrefixes">The command prefixes following <see cref="ExecutionAttribute.CommandName"/></param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class ExecutorAttribute(params string[] commandPrefixes) : Attribute
{
    /// <summary>
    /// The command prefixes following <see cref="ExecutionAttribute.CommandName"/>
    /// </summary>
    public string[] CommandPrefixes => commandPrefixes;
}
