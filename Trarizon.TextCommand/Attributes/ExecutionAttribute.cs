namespace Trarizon.TextCommand.Attributes;
/// <summary>
/// Execution entrance of the command
/// </summary>
/// <param name="commandName">Command prefix</param>
[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public sealed class ExecutionAttribute(string? commandName = null) : Attribute
{
    /// <summary>
    /// Command prefix
    /// </summary>
    public string? CommandName => commandName;

    /// <summary>
    /// Member method name of custom error handler
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>returns <see cref="void"/> or TReturn that can implicit convert to return type of execution</item>
    /// <item>first parameter is <see cref="Input.Result.ArgParsingErrors"/></item>
    /// <item>[optional] second parameter is <see cref="string"/>, means the executor method name</item>
    /// </list>
    /// </remarks>
    public string? ErrorHandler { get; init; }

    /// <summary>
    /// Member method name of custom matcher selector
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>returns ReadOnlySpan&lt;char&gt; or <see cref="string"/>, or any type allows list pattern</item>
    /// <item>single parameter matches the type of input parameter in execution</item>
    /// </list>
    /// </remarks>
    public string? MatcherSelector { get; init; }
}
