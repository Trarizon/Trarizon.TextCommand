namespace Trarizon.TextCommand.Attributes.Parameters;
[AttributeUsage(AttributeTargets.Parameter)]
public abstract class CLParameterAttribute : Attribute
{
    public string? Parser { get; init; }

    public Type? ParserType { get; init; }
}
