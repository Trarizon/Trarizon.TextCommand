namespace Trarizon.TextCommand.Attributes.Parameters;
/// <summary>
/// Context parameter, which directly get from execution
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class ContextParameterAttribute : Attribute
{
    /// <summary>
    /// Parameter name in execution method, can be <see langword="null"/> if
    /// the name in executor method is same with execution
    /// </summary>
    public string? ParameterName { get; init; }
}
