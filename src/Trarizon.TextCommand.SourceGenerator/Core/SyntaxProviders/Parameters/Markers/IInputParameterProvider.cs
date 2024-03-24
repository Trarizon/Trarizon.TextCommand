using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas.Markers;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.Parameters.Markers;
internal interface IInputParameterProvider : IParameterProvider
{
    new IInputParameterData Data { get; }

    IEnumerable<StatementSyntax> CaseBodyLocalStatements();
}
