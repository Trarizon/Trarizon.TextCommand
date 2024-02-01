using Microsoft.CodeAnalysis;
using Trarizon.TextCommand.SourceGenerator.Core.Models;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models.Parameters;
internal interface IParameterData
{
    ParameterModel Model { get; }

    ParserInfoProvider ParserInfo { get; }

    ITypeSymbol ParsedTypeSymbol { get; }
}
