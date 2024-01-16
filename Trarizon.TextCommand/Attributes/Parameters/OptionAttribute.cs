namespace Trarizon.TextCommand.Attributes.Parameters;
public sealed class OptionAttribute(string? alias = null) : CLParameterAttribute
{
    public string? Alias => alias;

    public string? Name { get; init; }

    public bool Required { get; init; }
}
