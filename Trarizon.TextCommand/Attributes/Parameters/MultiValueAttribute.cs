namespace Trarizon.TextCommand.Attributes.Parameters;
/// <summary>
/// Multi-value parameter, contains multiple value parameters in a collection
/// </summary>
/// <param name="maxCount">Max count of the collection, the collection will include all rest values if not set or <c>&lt; 0</c></param>
public sealed class MultiValueAttribute(int maxCount) : ParameterAttribute, IRequiredParameterAttribute
{
    /// <summary>
    /// Max count of the collection, the collection will include all rest values if <c>&lt; 0</c>
    /// </summary>
    public int MaxCount => maxCount;

    /// <inheritdoc />
    public bool Required { get; init; }

    /// <summary>
    /// Create a collection include all rest values
    /// </summary>
    public MultiValueAttribute() : this(0) { }
}
