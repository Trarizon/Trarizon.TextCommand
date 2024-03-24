using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Text;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models;
using Trarizon.TextCommand.SourceGenerator.Utilities;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;
using Trarizon.TextCommand.SourceGenerator.Utilities.Factories;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders;
internal sealed class CommandProvider(CommandModel model, ExecutionProvider execution)
{
    public CommandModel Model { get; } = model;

    public ExecutionProvider Execution { get; } = execution;

    // Decls

    public string GenerateFileName()
    {
        var display = Model.Symbol.ToDisplayString();
        return new StringBuilder(display, display.Length + 5)
            .Replace('<', '}')
            .Replace('>', '}')
            .Append(".g.cs").ToString();
    }

    public MemberDeclarationSyntax PartialTypeDeclaration()
    {
        return CodeFactory.CloneContainingTypeAndNamespaceDeclarations(
            Model.Syntax,
            Model.Symbol,
            SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                Execution.MethodDeclaration()));
    }

    public ClassDeclarationSyntax ParsingContextTypeDeclaration()
    {
        return SyntaxFactory.ClassDeclaration(
            SyntaxFactory.SingletonList(
                SyntaxProvider.GeneratedCodeAttributeSyntax),
            SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.FileKeyword),
                SyntaxFactory.Token(SyntaxKind.StaticKeyword)),
            SyntaxFactory.Identifier(Literals.G_ParsingContextProvider_TypeIdentifier),
            typeParameterList: null,
            baseList: null,
            constraintClauses: default,
            SyntaxFactory.List<MemberDeclarationSyntax>(
                Execution.Executors
                    .Select(e => e.ParsingContextFieldDeclaration())
                    .OfNotNull()));
    }
}
