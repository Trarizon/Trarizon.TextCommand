namespace Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
internal sealed class ContextParameterData(ParameterModel model) : IParameterData
{
    public ParameterModel Model { get; } = model;

    // Attribute datas

    private string? _parameterName;
    /// <summary>
    /// Name of parameter in execution
    /// </summary>
    public string ParameterName
    {
        get => _parameterName ??= Model.Symbol.Name;
        init => _parameterName = value;
    }
}
