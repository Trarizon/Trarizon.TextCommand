namespace Trarizon.TextCommand.Attributes.Parameters;
/// <summary>
/// Flag parameter
/// </summary>
/// <param name="alias">Alias(Short name) of parameter, use <c>-alias</c> to input</param>
public sealed class FlagAttribute(string? alias = null) : ParameterAttribute, INamedParameterAttribute
{
    /// <inheritdoc />
    public string? Alias => alias;

    /// <inheritdoc />
    public string? Name { get; init; }
}
