using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models;
using Trarizon.TextCommand.SourceGenerator.Core.Tags;
using Trarizon.TextCommand.SourceGenerator.Utilities;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders;
internal sealed class ExecutionProvider
{
    public ExecutionModel Model { get; }

    public CommandProvider Command { get; }

    public IReadOnlyList<ExecutorProvider> Executors { get; }

    public ExecutionProvider(ExecutionModel model)
    {
        Model = model;
        Command = new(Model.Command, this);
        Executors = Model.Executors
            .Select(executor => new ExecutorProvider(executor, this))
            .ToList();
    }

    // Literals
    private string? __input_ParameterIdentifier;
    public string L_Input_ParameterIdentifier() => __input_ParameterIdentifier ??= Model.Symbol.Parameters[0].Name;

    private string LocalVarIdentifier(string identifier)
    {
        while (Model.Symbol.Parameters.Any(p => p.Name == identifier)) {
            identifier += '_';
        }
        return identifier;
    }

    // Decls

    public MethodDeclarationSyntax MethodDeclaration()
    {
        return SyntaxFactory.MethodDeclaration(
            SyntaxFactory.SingletonList(
                SyntaxProvider.GeneratedCodeAttributeSyntax),
            Model.Syntax.Modifiers,
            SyntaxProvider.FullQualifiedIdentifierName(Model.Symbol.ReturnType),
            explicitInterfaceSpecifier: null, // partial method
            SyntaxFactory.Identifier(Model.Symbol.Name),
            Model.Syntax.TypeParameterList,
            SyntaxFactory.ParameterList(
                SyntaxFactory.SeparatedList(
                    Model.Symbol.Parameters.Select(parameter =>
                    {
                        var syntax = (ParameterSyntax)parameter.DeclaringSyntaxReferences[0].GetSyntax();
                        return SyntaxFactory.Parameter(
                            attributeLists: default,
                            syntax.Modifiers,
                            SyntaxProvider.FullQualifiedIdentifierName(parameter.Type),
                            SyntaxFactory.Identifier(parameter.Name),
                            @default: null);
                    }))),
            Model.Syntax.ConstraintClauses, // TODO:
            SyntaxFactory.Block(
                SyntaxFactory.List(
                    MethodBodyStatements())),
            expressionBody: null,
            semicolonToken: default);
    }

    private IEnumerable<StatementSyntax> MethodBodyStatements()
    {
        ExpressionSyntax? governing;

        // var customMatcher = CustomMatcher(input);
        // var stringMatcher = new StringInputMatcher(input);
        {
            var ipk = Model.InputParameter.Kind;
            string argName = Model.InputParameter.Symbol.Name;

        MatcherGenerating:
            switch (ipk) {
                case InputParameterKind.CustomMatcher: {
                    var customMatcherVarIdentifier = LocalVarIdentifier(Literals.G_CustomMatcher_VarIdentifier);
                    yield return SyntaxProvider.LocalVarSingleVariableDeclaration(
                        customMatcherVarIdentifier,
                        SyntaxFactory.InvocationExpression(
                            SyntaxProvider.SiblingMemberAccessExpression(Model.CustomMatcherSelector.Symbol),
                            SyntaxProvider.ArgumentList(
                                SyntaxFactory.IdentifierName(argName))));
                    ipk = Model.CustomMatcherSelector.ReturnInputParameterKind;
                    argName = customMatcherVarIdentifier;
                    goto MatcherGenerating;
                }
                case InputParameterKind.String:
                    var stringMatcherVarIdentifier = LocalVarIdentifier(Literals.G_StringInputMatcher_VarIdentifier);
                    yield return SyntaxProvider.LocalVarSingleVariableDeclaration(
                        stringMatcherVarIdentifier,
                        SyntaxFactory.ObjectCreationExpression(
                            SyntaxFactory.IdentifierName($"{Constants.Global}::{Literals.StringInputMatcher_TypeName}"),
                            SyntaxProvider.ArgumentList(
                                SyntaxFactory.IdentifierName(L_Input_ParameterIdentifier())),
                            initializer: default));
                    governing = SyntaxFactory.IdentifierName(stringMatcherVarIdentifier);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        // switch (input) { }
        yield return SyntaxFactory.SwitchStatement(
            governing,
            SyntaxFactory.List(
                Executors.SelectMany(e => e.GenerateMatchingSwitchSections())));

        // return default;
        if (Model.Symbol.ReturnsVoid)
            yield return SyntaxFactory.ReturnStatement();
        else
            yield return SyntaxFactory.ReturnStatement(
                SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression));
    }
}
