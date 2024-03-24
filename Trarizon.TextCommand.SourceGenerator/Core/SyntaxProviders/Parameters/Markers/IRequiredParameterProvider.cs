using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas.Markers;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.Parameters.Markers;
internal interface IRequiredParameterProvider : IInputParameterProvider
{
    new IRequiredParameterData Data { get; }
}
