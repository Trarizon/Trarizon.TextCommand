using Microsoft.CodeAnalysis;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
internal interface IParameterData
{
    ParameterModel Model { get; }
    ParserInfoProvider ParserInfo { get; }
    ITypeSymbol ParsedTypeSymbol { get; }
}
