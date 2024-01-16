using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models;
internal sealed class ContextModel
{
    public readonly SemanticModel SemanticModel;
    public readonly ExecutionModel ExecutionModel;
    public readonly CommandModel CommandModel;

    public ContextModel(
        in GeneratorAttributeSyntaxContext context,
        TypeDeclarationSyntax commandTypeSyntax)
    {
        SemanticModel = context.SemanticModel;
        ExecutionModel = new(this,
            (MethodDeclarationSyntax)context.TargetNode,
            (IMethodSymbol)context.TargetSymbol,
            context.Attributes[0]);
        CommandModel = new(commandTypeSyntax, SemanticModel.GetDeclaredSymbol(commandTypeSyntax)!);
    }
}
