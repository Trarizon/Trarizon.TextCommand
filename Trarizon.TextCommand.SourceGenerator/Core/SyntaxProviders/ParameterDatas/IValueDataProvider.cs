using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.ParameterDatas;
internal interface IValueDataProvider : IInputParameterDataProvider
{
    new IValueParameterData Data { get; }
}
