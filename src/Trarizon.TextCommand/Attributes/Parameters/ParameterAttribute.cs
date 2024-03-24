namespace Trarizon.TextCommand.Attributes.Parameters;
/// <summary>
/// Base type of parameters
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public abstract class ParameterAttribute : Attribute
{
    /// <summary>
    /// member name of custom parser
    /// </summary>
    public string? Parser { get; init; }

    /// <summary>
    /// type of custom parser
    /// </summary>
    public Type? ParserType { get; init; }
}
