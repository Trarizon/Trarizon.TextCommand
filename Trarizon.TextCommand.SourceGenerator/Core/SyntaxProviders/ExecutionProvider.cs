﻿using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models;
using Trarizon.TextCommand.SourceGenerator.Core.Tags;
using Trarizon.TextCommand.SourceGenerator.Utilities;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders;
internal sealed class ExecutionProvider
{
    public ExecutionModel Model { get; }

    public CommandProvider Command { get; }

    public IReadOnlyList<ExecutorProvider> Executors { get; }

    public ExecutionProvider(ExecutionModel model, CommandProvider command)
    {
        Model = model;
        Command = command;
        Executors = model.Executors.SelectNonException(exec => new ExecutorProvider(exec, this)).ToList();
    }

    // Literals

    private string? _input_ParameterIdentifier;
    public string Input_ParameterIdentifier() => _input_ParameterIdentifier ??= Model.Syntax.ParameterList.Parameters[0].Identifier.Text;

    // Decls

    public MethodDeclarationSyntax MethodDeclaration()
    {
        return SyntaxFactory.MethodDeclaration(
            SyntaxFactory.SingletonList(
                SyntaxProvider.GeneratedCodeAttributeSyntax),
            Model.Syntax.Modifiers,
            Model.Syntax.ReturnType,
            Model.Syntax.ExplicitInterfaceSpecifier,
            Model.Syntax.Identifier,
            Model.Syntax.TypeParameterList,
            Model.Syntax.ParameterList,
            Model.Syntax.ConstraintClauses,
            SyntaxFactory.Block(
                SyntaxFactory.List(
                    MethodBodyStatements())),
            default,
            semicolonToken: default);
    }

    private IEnumerable<StatementSyntax> MethodBodyStatements()
    {
        ExpressionSyntax? governing;

        // if string:
        // var __matcher = new();
        switch (Model.InputParameterType) {
            case InputParameterType.String:
                yield return SyntaxProvider.LocalVarSingleVariableDeclaration(
                    Literals.StringInputMatcher_VarIdentifier,
                    SyntaxFactory.ObjectCreationExpression(
                        SyntaxFactory.IdentifierName($"{Constants.Global}::{Literals.StringInputMatcher_TypeName}"),
                        SyntaxProvider.ArgumentList(
                            SyntaxFactory.IdentifierName(Input_ParameterIdentifier())),
                        default));
                governing = SyntaxFactory.IdentifierName(Literals.StringInputMatcher_VarIdentifier);
                break;
            default:
                throw new InvalidOperationException();
        }

        // switch() { }
        yield return SyntaxFactory.SwitchStatement(
            governing,
            SyntaxFactory.List(
                Executors.SelectMany(e => e.MatchingSwitchSections())));

        yield return ReturnStatement();
    }

    private ReturnStatementSyntax ReturnStatement()
    {
        if (Model.Symbol.ReturnsVoid)
            return SyntaxFactory.ReturnStatement();

        return SyntaxFactory.ReturnStatement(
            SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression));
    }
}
