using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models;
internal sealed class CommandModel(ExecutionModel execution)
{
    public required TypeDeclarationSyntax Syntax { get; init; }
    public required INamedTypeSymbol Symbol { get; init; }

    public ExecutionModel Execution { get; } = execution;
}
