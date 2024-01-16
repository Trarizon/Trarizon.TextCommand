namespace Trarizon.TextCommand.Attributes.Parameters;
public sealed class MultiValueAttribute(int maxCount) : CLParameterAttribute
{
    public int MaxCount => maxCount;

    public bool Required { get; init; }

    public MultiValueAttribute() : this(0) { }
}
