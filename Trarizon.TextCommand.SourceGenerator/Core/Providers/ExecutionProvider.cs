using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models;
using Trarizon.TextCommand.SourceGenerator.Utilities;

namespace Trarizon.TextCommand.SourceGenerator.Core.Providers;
internal sealed class ExecutionProvider
{
    private readonly ExecutionModel _model;

    public CommandProvider Command { get; }

    public ExecutorProvider[] Executors { get; }

    public string? CommandPrefix { get; }

    public ExecutionProvider(ExecutionModel model)
    {
        _model = model;
        Command = new CommandProvider(this, model.Command);
        Executors = new ExecutorProvider[model.Executors.Count];
        for (int i = 0; i < Executors.Length; i++) {
            Executors[i] = new ExecutorProvider(this, model.Executors[i]);
        }
        CommandPrefix = model.CommandName;
    }

    // Syntax
    public string InputParameter_Identifier => _model.Symbol.Parameters[0].Name;

    public MethodDeclarationSyntax MethodDeclaration()
    {
        return SyntaxFactory.MethodDeclaration(
            SyntaxFactory.SingletonList(
                SyntaxProvider.GeneratedCodeAttributeSyntax),
            _model.Syntax.Modifiers,
            _model.Syntax.ReturnType,
            _model.Syntax.ExplicitInterfaceSpecifier,
            _model.Syntax.Identifier,
            _model.Syntax.TypeParameterList,
            _model.Syntax.ParameterList,
            _model.Syntax.ConstraintClauses,
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
        switch (_model.InputParameterType) {
            case InputParameterType.String:
                yield return SyntaxProvider.LocalVarSingleVariableDeclaration(
                    Literals.StringInputMatcher_VarIdentifier,
                    SyntaxFactory.ObjectCreationExpression(
                        SyntaxFactory.IdentifierName($"{Constants.Global}::{Literals.StringInputMatcher_TypeName}"),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Argument(
                                    SyntaxFactory.IdentifierName(
                                        InputParameter_Identifier)))),
                        default));
                governing = SyntaxFactory.IdentifierName(Literals.StringInputMatcher_VarIdentifier);
                break;
            case InputParameterType.Unknown:
            case InputParameterType.Span:
                governing = SyntaxFactory.IdentifierName(InputParameter_Identifier);
                break;
            case InputParameterType.Array:
                governing = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.IdentifierName(($"{Constants.Global}::{Constants.MemoryExtensions_TypeName}.{Constants.AsSpan_Identifier}")),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Argument(
                                SyntaxFactory.IdentifierName(InputParameter_Identifier)))));
                break;
            case InputParameterType.List:
                governing = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.IdentifierName(($"{Constants.Global}::{Constants.CollectionsMarshal_TypeName}.{Constants.AsSpan_Identifier}")),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Argument(
                                SyntaxFactory.IdentifierName(InputParameter_Identifier)))));
                break;
            default:
                throw new InvalidOperationException();
        }

        // switch () { }
        yield return SyntaxFactory.SwitchStatement(
            governing,
            SyntaxFactory.List(
                Executors.Select(e => e.MatchingSwitchSection())));

        yield return ReturnStatement();
    }

    private ReturnStatementSyntax ReturnStatement()
    {
        if (_model.Symbol.ReturnsVoid)
            return SyntaxFactory.ReturnStatement();
        else
            return SyntaxFactory.ReturnStatement(
                SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression));
    }

    public ClassDeclarationSyntax ParameterSets_ClassDeclaration()
    {
        return SyntaxFactory.ClassDeclaration(
            SyntaxFactory.SingletonList(
                SyntaxProvider.GeneratedCodeAttributeSyntax),
            SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.FileKeyword),
                SyntaxFactory.Token(SyntaxKind.StaticKeyword)),
            SyntaxFactory.Identifier(Literals.ParameterSets_TypeIdentifier),
            typeParameterList: null,
            baseList: null,
            constraintClauses: default,
            SyntaxFactory.List<MemberDeclarationSyntax>(
                Executors.Select(e => e.ParameterSet_FieldDeclaration())));
    }
}