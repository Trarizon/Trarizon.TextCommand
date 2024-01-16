namespace Trarizon.TextCommand.Attributes.Parameters;
public sealed class FlagAttribute(string? alias = null) : CLParameterAttribute
{
    public string? Alias => alias;

    public string? Name { get; init; }
}
