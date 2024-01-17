using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Trarizon.TextCommand.SourceGenerator.Core.Models;
using Trarizon.TextCommand.SourceGenerator.Utilities.Factories;

namespace Trarizon.TextCommand.SourceGenerator.Core.Providers;
internal class CommandProvider(ExecutionProvider execution, CommandModel model)
{
    public INamedTypeSymbol Symbol => model.Symbol;

    public MemberDeclarationSyntax ClonePartialTypeDeclaration()
    {
        return CodeFactory.CloneContainingTypeAndNamespaceDeclarations(
            model.Syntax,
            model.Symbol,
            SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                execution.MethodDeclaration()));
    }
}
