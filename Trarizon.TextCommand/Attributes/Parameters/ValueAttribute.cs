namespace Trarizon.TextCommand.Attributes.Parameters;
/// <summary>
/// Value parameter
/// </summary>
public sealed class ValueAttribute : ParameterAttribute, IRequiredParameterAttribute
{
    /// <inheritdoc />
    public bool Required { get; init; }
}
