namespace Trarizon.TextCommand.SourceGenerator.Core.Models.Parameters;
internal interface IRequiredParameterData : IParameterData
{
    bool Required { get; }
}
