namespace Trarizon.TextCommand.Attributes.Parameters;
internal interface INamedParameterAttribute
{
    /// <summary>
    /// Alias(Short name) of parameter, use <c>-alias</c> to input
    /// </summary>
    string? Alias { get; }

    /// <summary>
    /// Name(Full name) of parameter, use <c>--name</c> to input
    /// </summary>
    string? Name { get; }
}

internal interface IRequiredParameterAttribute
{
    /// <summary>
    /// Indicate if the parameter is required.
    /// </summary>
    /// <remarks>
    /// If <see langword="true"/>, execution will create error when parameter is not set;<br/>
    /// else, execution only create error when parsing parameter failed.
    /// </remarks>
    bool Required { get; }
}
