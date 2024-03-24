using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models;
using Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.Parameters.Markers;
using Trarizon.TextCommand.SourceGenerator.Core.Tags;
using Trarizon.TextCommand.SourceGenerator.Utilities;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders;
internal class ExecutorProvider
{
    public ExecutorModel Model { get; }

    public ExecutionProvider Execution { get; }

    public IReadOnlyList<IParameterProvider?> Parameters { get; }

    public ExecutorProvider(ExecutorModel model, ExecutionProvider execution)
    {
        Model = model;
        Execution = execution;
        Parameters = model.Parameters
            .Select(parameter => ParameterProvider.Create(parameter, this))
            .ToList();
        Debug.Assert(Model.Symbol.Parameters.Length == Parameters.Count);
    }

    // Literals

    private string LocalIdentifier(string prefix) => $"{prefix}_{Model.Symbol.Name}";

    public string L_RestArgs_VarIdentifier(int executorIndex) => $"{Literals.G_RestArg_VarIdentifier}_{Model.Symbol.Name}{executorIndex}";
    public string L_ParsingContext_FieldIdentifier() => Model.Symbol.Name;
    public string L_ArgsProvider_VarIdentifier() => LocalIdentifier(Literals.G_ArgsProvider_VarIdentifier);
    public string L_LabelIdentifier() => LocalIdentifier(Literals.G_Executor_GotoLabelIdentifier);
    public string L_ErrorsBuilder_VarIdentifier() => LocalIdentifier(Literals.G_ErrorsBuilder_VarIdentifier);

    // Decls

    /// <summary>
    /// [
    /// case [..]: statements;
    /// ]
    /// </summary>
    public IEnumerable<SwitchSectionSyntax> GenerateMatchingSwitchSections()
    {
        if (Parameters.Count > 0) {
            // case [..]:
            //     statements;
            // case [..]:
            //     goto;
            return Model.CommandPrefixes.Select((_, i) =>
            {
                return SyntaxFactory.SwitchSection(
                    SyntaxFactory.SingletonList(
                        GenerateCaseLabel(i)),
                    SyntaxFactory.List(
                        GenerateCaseStatments(i)));
            });
        }
        else {
            // case [..]:
            // case [..]:
            //     statements;
            return SyntaxFactory.SwitchSection(
                SyntaxFactory.List(
                    Model.CommandPrefixes.Select((_, i) => GenerateCaseLabel(i))),
                SyntaxFactory.List(
                    GenerateCaseStatments(0)))
                .Collect();
        }
    }

    /// <summary>
    /// case [..]:
    /// </summary>
    private SwitchLabelSyntax GenerateCaseLabel(int executorIndex)
    {
        return SyntaxFactory.CasePatternSwitchLabel(
            SyntaxFactory.ListPattern(
                SyntaxFactory.SeparatedList(
                    ListPatternSubPattern(executorIndex))),
            SyntaxFactory.Token(SyntaxKind.ColonToken));
    }

    /// <summary>
    /// "cmd", "cmd", .. var rest
    /// </summary>
    private IEnumerable<PatternSyntax> ListPatternSubPattern(int executorIndex)
    {
        foreach (var cmdPrefix in Execution.Model.CommandNames.Concat(Model.CommandPrefixes[executorIndex])) {
            yield return SyntaxFactory.ConstantPattern(
                SyntaxProvider.LiteralStringExpression(cmdPrefix));
        }

        if (Parameters.Count == 0) {
            yield return SyntaxFactory.SlicePattern();
        }
        else {
            yield return SyntaxFactory.SlicePattern(
                SyntaxFactory.VarPattern(
                    SyntaxFactory.SingleVariableDesignation(
                        SyntaxFactory.Identifier(L_RestArgs_VarIdentifier(executorIndex)))));
        }
    }

    /// <summary>
    /// Statements in case
    /// </summary>
    private IEnumerable<StatementSyntax> GenerateCaseStatments(int executorIndex)
    {
        if (Parameters.Count > 0) {
            // (var) provider = ParsingContext.Executor.Parse(restArgs)
            yield return ArgsProviderLocalVarStatement(executorIndex);
        }

        if (executorIndex == 0) {
            foreach (var s in MainCaseStatements())
                yield return s;
        }
        else {
            yield return SyntaxFactory.GotoStatement(
                SyntaxKind.GotoStatement,
                SyntaxFactory.IdentifierName(L_LabelIdentifier()));
        }

        IEnumerable<StatementSyntax> MainCaseStatements()
        {
            if (Parameters.Count > 0) {
                // errorBuilder = new();
                yield return SyntaxFactory.LabeledStatement(
                    L_LabelIdentifier(),
                    ErrorsBuilderLocalVarStatement());

                // var args = provider.Get<>()
                // ...
                foreach (var statement in Parameters
                    .OfType<IInputParameterProvider>()
                    .SelectMany(parameter => parameter.CaseBodyLocalStatements()))
                    yield return statement;

                // if (builder.HasError)
                //     return ErrorHandler();
                yield return SyntaxFactory.IfStatement(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(L_ErrorsBuilder_VarIdentifier()),
                        SyntaxFactory.IdentifierName(Literals.ArgParsingErrorsBuilder_HasError_PropertyIdentifier)),
                    SyntaxFactory.Block(
                        SyntaxFactory.List(
                            ErrorHandlingStatements())));
            }

            var invocation = SyntaxFactory.InvocationExpression(
                SyntaxProvider.SiblingMemberAccessExpression(Model.Symbol),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(Parameters.Select(p =>
                    {
                        if (p is not null) {
                            return p.ExecutorArgAccess_ArgumentSyntax();
                        }
                        return SyntaxFactory.Argument(
                            SyntaxProvider.LiteralDefaultExpression());
                    }))));

            if (Model.Symbol.ReturnsVoid) {
                yield return SyntaxFactory.ExpressionStatement(invocation);
                yield return SyntaxFactory.ReturnStatement();
            }
            else {
                yield return SyntaxFactory.ReturnStatement(invocation);
            }
        }
    }

    private StatementSyntax ArgsProviderLocalVarStatement(int executorIndex)
    {
        var invocation = SyntaxProvider.SimpleMethodInvocation(
            SyntaxFactory.IdentifierName($"{Constants.Global}::{Literals.G_ParsingContextProvider_TypeIdentifier}.{L_ParsingContext_FieldIdentifier()}"),
            SyntaxFactory.IdentifierName(Literals.ParsingContext_Parse_MethodIdentifier),
            SyntaxFactory.IdentifierName(L_RestArgs_VarIdentifier(executorIndex)));

        if (executorIndex == 0) {
            return SyntaxProvider.LocalVarSingleVariableDeclaration(
                L_ArgsProvider_VarIdentifier(),
                invocation);
        }
        else {
            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(L_ArgsProvider_VarIdentifier()),
                    invocation));
        }
    }

    private StatementSyntax ErrorsBuilderLocalVarStatement()
    {
        return SyntaxProvider.LocalVarSingleVariableDeclaration(
            L_ErrorsBuilder_VarIdentifier(),
            SyntaxFactory.ObjectCreationExpression(
                SyntaxFactory.IdentifierName($"{Constants.Global}::{Literals.ArgParsingErrorsBuilder_TypeName}"),
                SyntaxFactory.ArgumentList(),
                null));
    }

    private IEnumerable<StatementSyntax> ErrorHandlingStatements()
    {
        var (errorHandlerKind, errorHandler) = Execution.Model.CustomErrorHandler;

        InvocationExpressionSyntax invocation;
        switch (errorHandlerKind) {
            case ErrorHandlerKind.Invalid: {
                // builder.DefaultErrorHandler()
                // break;
                yield return SyntaxFactory.ExpressionStatement(
                    SyntaxProvider.SimpleMethodInvocation(
                        SyntaxFactory.IdentifierName(L_ErrorsBuilder_VarIdentifier()),
                        SyntaxFactory.IdentifierName(Literals.ArgParsingErrorsBuilder_DefaultErrorHandler_MethodIdentifier)));
                yield return SyntaxFactory.BreakStatement();
                yield break;
            }
            case ErrorHandlerKind.Minimal: {
                invocation = SyntaxFactory.InvocationExpression(
                    SyntaxProvider.SiblingMemberAccessExpression(errorHandler),
                    SyntaxProvider.ArgumentList(ErrorsArgExpr()));
                break;
            }
            case ErrorHandlerKind.WithExecutorName: {
                invocation = SyntaxFactory.InvocationExpression(
                    SyntaxProvider.SiblingMemberAccessExpression(errorHandler),
                    SyntaxProvider.ArgumentList(
                        ErrorsArgExpr(),
                        SyntaxProvider.LiteralStringExpression(Model.Symbol.Name)));
                break;
            }
            default:
                throw new InvalidOperationException();
        }

        if (errorHandler.ReturnsVoid) {
            yield return SyntaxFactory.ExpressionStatement(invocation);
            yield return SyntaxFactory.BreakStatement();
        }
        else {
            yield return SyntaxFactory.ReturnStatement(invocation);
        }

        ExpressionSyntax ErrorsArgExpr()
        {
            return SyntaxProvider.SimpleMethodInvocation(
                SyntaxFactory.IdentifierName(L_ErrorsBuilder_VarIdentifier()),
                SyntaxFactory.IdentifierName(Literals.ArgParsingErrorsBuilder_Build_MethodIdentifier),
                SyntaxFactory.IdentifierName(L_ArgsProvider_VarIdentifier()));
        }
    }

    public FieldDeclarationSyntax? ParsingContextFieldDeclaration()
    {
        if (Parameters.Count == 0)
            return null;


        return SyntaxFactory.FieldDeclaration(
            SyntaxFactory.SingletonList(
                SyntaxProvider.GeneratedCodeAttributeSyntax),
            SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.StaticKeyword),
                SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)),
            SyntaxFactory.VariableDeclaration(
                SyntaxFactory.IdentifierName($"{Constants.Global}::{Literals.ParsingContext_TypeName}"),
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.VariableDeclarator(
                        SyntaxFactory.Identifier(L_ParsingContext_FieldIdentifier()),
                        default,
                        SyntaxFactory.EqualsValueClause(
                            ParsingContextObjectCreationExpr())))));
    }

    private ImplicitObjectCreationExpressionSyntax ParsingContextObjectCreationExpr()
    {
        var namedParameter = Parameters
            .OfType<INamedParameterProvider>()
            .ToList();

        return SyntaxFactory.ImplicitObjectCreationExpression(
            SyntaxProvider.ArgumentList(
                DictParameterArgExpression(
                    Parameters.OfType<INamedParameterProvider>(),
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
                    p => SyntaxProvider.LiteralStringExpression(p.Data.Name),
                    p => p.ParsingContextArgDictValueExpr()),
                DictParameterArgExpression(
                    Parameters.OfType<INamedParameterProvider>().Where(p => p.Data.Alias is not null),
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                    p => SyntaxProvider.LiteralStringExpression(p.Data.Alias!),
                    p => SyntaxProvider.LiteralStringExpression(p.Data.Name))),
            default);

        static ExpressionSyntax DictParameterArgExpression(
            IEnumerable<INamedParameterProvider> providers,
            TypeSyntax dictKeyType,
            TypeSyntax dictValueType,
            Func<INamedParameterProvider, ExpressionSyntax> initializerKeySelector,
            Func<INamedParameterProvider, ExpressionSyntax> initializerValueSelector)
        {
            var dictType = SyntaxFactory.GenericName(
                SyntaxFactory.Identifier($"{Constants.Global}::{Constants.Dictionary_TypeName}"),
                SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SeparatedList(new[] {
                        dictKeyType,
                        dictValueType,
                    })));

            var items = providers.Select(p =>
            {
                return SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.ImplicitElementAccess(
                        SyntaxFactory.BracketedArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Argument(initializerKeySelector(p))))),
                    initializerValueSelector(p));
            }).ToList();

            if (items.Count == 0)
                return SyntaxFactory.DefaultExpression(dictType);

            return SyntaxFactory.ObjectCreationExpression(
                dictType,
                SyntaxProvider.ArgumentList(
                    SyntaxProvider.LiteralInt32Expression(items.Count)),
                SyntaxFactory.InitializerExpression(
                    SyntaxKind.ObjectInitializerExpression,
                    SyntaxFactory.SeparatedList<ExpressionSyntax>(items)));
        }
    }
}
