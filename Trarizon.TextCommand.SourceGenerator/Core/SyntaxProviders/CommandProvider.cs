﻿using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models;
using Trarizon.TextCommand.SourceGenerator.Utilities;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;
using Trarizon.TextCommand.SourceGenerator.Utilities.Factories;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders;
internal sealed class CommandProvider
{
    public CommandModel Model { get; }

    public ExecutionProvider Execution { get; }

    public CommandProvider(CommandModel model)
    {
        Model = model;
        Execution = new ExecutionProvider(model.ExecutionModel, this);
    }

    // Decls

    public string GeneratedFileName()
        => $"{Model.Symbol.ToDisplayString().Replace('<', '}').Replace('>', '}')}.g.cs";

    public MemberDeclarationSyntax PartialTypeDeclaration()
    {
        return CodeFactory.CloneContainingTypeAndNamespaceDeclarations(
            Model.Syntax,
            Model.Symbol,
            SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                Execution.MethodDeclaration()));
    }

    public ClassDeclarationSyntax ParameterSetsTypeDeclaration()
    {
        return SyntaxFactory.ClassDeclaration(
            SyntaxFactory.SingletonList(
                SyntaxProvider.GeneratedCodeAttributeSyntax),
            SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.FileKeyword),
                SyntaxFactory.Token(SyntaxKind.StaticKeyword)),
            SyntaxFactory.Identifier(Literals.ParsingContextProvider_TypeIdentifier),
            typeParameterList: null,
            baseList: null,
            constraintClauses: default,
            SyntaxFactory.List<MemberDeclarationSyntax>(
                Execution.Executors
                    .Select(e => e.ParameterSetFieldDeclaration())
                    .OfNotNull()));
    }
}
