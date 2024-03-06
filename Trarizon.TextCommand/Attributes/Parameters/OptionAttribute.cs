namespace Trarizon.TextCommand.Attributes.Parameters;
/// <summary>
/// Option parameter
/// </summary>
/// <param name="alias">Alias(Short name) of parameter, use <c>-alias</c> to input</param>
public sealed class OptionAttribute(string? alias = null) : ParameterAttribute, INamedParameterAttribute, IRequiredParameterAttribute
{
    /// <inheritdoc />
    public string? Alias => alias;

    /// <inheritdoc />
    public string? Name { get; init; }

    /// <inheritdoc />
    public bool Required { get; init; }
}
