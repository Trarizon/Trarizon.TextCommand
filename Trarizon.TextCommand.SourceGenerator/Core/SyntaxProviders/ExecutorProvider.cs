using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models;
using Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.ParameterDatas;
using Trarizon.TextCommand.SourceGenerator.Utilities;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders;
internal class ExecutorProvider
{
    public ExecutorModel Model { get; }

    public ExecutionProvider Execution { get; }

    public ImmutableArray<ParameterProvider> Parameters { get; }

    /// <remarks>
    /// May throw
    /// </remarks>
    public ExecutorProvider(ExecutorModel model, ExecutionProvider execution)
    {
        Model = model;
        Execution = execution;

        Parameters = model.Parameters
            .Select(parameter => new ParameterProvider(parameter, this))
            .ToImmutableArray();
    }

    public string RestArgs_VarIdentifier(int? suffix) => $"__rest_{Model.Symbol.Name}{suffix}";
    public string ParameterSet_FieldIdentifier() => Model.Symbol.Name;
    public string ArgsProvider_VarIdentifier() => $"__provider_{Model.Symbol.Name}";
    public string ErrorsBuilder_VarIdentifier() => $"__builder_{Model.Symbol.Name}";
    public string MainExecutor_LabelIdentifier() => $"__MAIN_EXECUTOR_{Model.Symbol.Name}";

    public IEnumerable<SwitchSectionSyntax> MatchingSwitchSections()
    {
        if (Model.CommandPrefixes.Length > 1 && Parameters.Length > 0)
            return MultiMatchingSwitchSections();
        else
            return [SingleMatchingSwitchSection()];

        // More than 1 [Executor], we use goto to jump
        IEnumerable<SwitchSectionSyntax> MultiMatchingSwitchSections()
        {
            // case [..]:
            // __Label:
            //     return Executor();
            yield return SyntaxFactory.SwitchSection(
                SyntaxFactory.SingletonList<SwitchLabelSyntax>(
                    SyntaxFactory.CasePatternSwitchLabel(
                        SyntaxFactory.ListPattern(
                            SyntaxFactory.SeparatedList(
                                ListPatternSubPatterns(Model.CommandPrefixes[0]))),
                        SyntaxFactory.Token(SyntaxKind.ColonToken))),
                SyntaxFactory.List(
                    MainCaseStatements()));

            // case [..]:
            //     goto __Label;
            for (int i = 1; i < Model.CommandPrefixes.Length; i++) {
                yield return SyntaxFactory.SwitchSection(
                    SyntaxFactory.SingletonList<SwitchLabelSyntax>(
                        SyntaxFactory.CasePatternSwitchLabel(
                            SyntaxFactory.ListPattern(
                                SyntaxFactory.SeparatedList(
                                    ListPatternSubPatterns(Model.CommandPrefixes[i], i))),
                            SyntaxFactory.Token(SyntaxKind.ColonToken))),
                    SyntaxFactory.List(
                        SubCaseStatements(i)));
            }
        }

        SwitchSectionSyntax SingleMatchingSwitchSection()
        {
            return SyntaxFactory.SwitchSection(
                SyntaxFactory.List<SwitchLabelSyntax>(
                    Model.CommandPrefixes.Select(prefixes =>
                    {
                        return SyntaxFactory.CasePatternSwitchLabel(
                            SyntaxFactory.ListPattern(
                                    SyntaxFactory.SeparatedList(
                                        ListPatternSubPatterns(prefixes))),
                            SyntaxFactory.Token(SyntaxKind.ColonToken));
                    })),
                SyntaxFactory.List(
                    MainCaseStatements()));
        }
    }

    private IEnumerable<PatternSyntax> ListPatternSubPatterns(string[] executorCommandPrefixes, int? restIdentifierSuffix = default)
    {
        // Execution part
        if (Execution.Model.CommandName is not null) {
            yield return SyntaxFactory.ConstantPattern(
                SyntaxProvider.LiteralStringExpression(Execution.Model.CommandName));
        }

        // Executor part
        foreach (var prefix in executorCommandPrefixes) {
            yield return SyntaxFactory.ConstantPattern(
                SyntaxProvider.LiteralStringExpression(prefix));
        }

        if (Parameters.Length == 0) {
            // ..
            yield return SyntaxFactory.SlicePattern();
        }
        else {
            // .. var __rest
            yield return SyntaxFactory.SlicePattern(
                SyntaxFactory.VarPattern(
                    SyntaxFactory.SingleVariableDesignation(
                        SyntaxFactory.Identifier(RestArgs_VarIdentifier(restIdentifierSuffix)))));
        }
    }

    private IEnumerable<StatementSyntax> MainCaseStatements()
    {
        if (Parameters.Length > 0) {
            // _provider = ParamSet.Exec.Parse(__rest)
            yield return ArgsProviderLocalVarStatement(true);

            // _builder = new();
            yield return SyntaxFactory.LabeledStatement(
                MainExecutor_LabelIdentifier(),
                ErrorsBuilderLocalVarStatement());

            foreach (var parameter in Parameters) {
                // var args = provider.Get<>();
                yield return parameter.ArgumentLocalDeclaration();

                foreach (var stat in parameter.ArgumentLocalExtraStatments()) {
                    yield return stat;
                }
            }

            // if (builder.HasError)
            //     return ErrorHandler();
            yield return SyntaxFactory.IfStatement(
               SyntaxFactory.MemberAccessExpression(
                   SyntaxKind.SimpleMemberAccessExpression,
                   SyntaxFactory.IdentifierName(ErrorsBuilder_VarIdentifier()),
                   SyntaxFactory.IdentifierName(Literals.ArgParsingErrorsBuilder_HasError_PropertyIdentifier)),
               SyntaxFactory.Block(
                   SyntaxFactory.List(
                       ErrorHandlingStatements())));
        }

        var invocation = SyntaxFactory.InvocationExpression(
            SyntaxProvider.SiblingMemberAccessExpression(Model.Symbol),
            SyntaxProvider.ArgumentList(Parameters.Select(p => p.ParameterData.GetResultValueAccessExpression())));

        // return statements
        if (Execution.Model.Symbol.ReturnsVoid) {
            // Method(...);
            // return;
            yield return SyntaxFactory.ExpressionStatement(invocation);
            yield return SyntaxFactory.ReturnStatement();
        }
        else {
            // return Method(...);
            yield return SyntaxFactory.ReturnStatement(invocation);
        }
    }

    private StatementSyntax[] SubCaseStatements(int? restIdentifierSuffix) => [
        ArgsProviderLocalVarStatement(false,restIdentifierSuffix),
        SyntaxFactory.GotoStatement(
            SyntaxKind.GotoStatement,
            SyntaxFactory.IdentifierName(MainExecutor_LabelIdentifier())),
    ];

    /// <summary>
    /// var __provider = Parse()
    /// __provider = Parse()
    /// </summary>
    private StatementSyntax ArgsProviderLocalVarStatement(bool declartion, int? restIdentifiersuffix = null)
    {
        var invocation = SyntaxProvider.SimpleMethodInvocation(
            SyntaxFactory.IdentifierName($"{Constants.Global}::{Literals.ParsingContextProvider_TypeIdentifier}.{ParameterSet_FieldIdentifier()}"),
            SyntaxFactory.IdentifierName(Literals.ParsingContext_Parse_MethodIdentifier),
            SyntaxFactory.IdentifierName(RestArgs_VarIdentifier(restIdentifiersuffix)));

        if (declartion) {
            return SyntaxProvider.LocalVarSingleVariableDeclaration(
                ArgsProvider_VarIdentifier(),
                invocation);
        }
        else {
            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(ArgsProvider_VarIdentifier()),
                    invocation));
        }
    }

    private StatementSyntax ErrorsBuilderLocalVarStatement()
    {
        return SyntaxProvider.LocalVarSingleVariableDeclaration(
            ErrorsBuilder_VarIdentifier(),
            SyntaxFactory.ObjectCreationExpression(
                SyntaxFactory.IdentifierName($"{Constants.Global}::{Literals.ArgParsingErrorsBuilder_TypeName}"),
                SyntaxFactory.ArgumentList(),
                null));
    }

    private IEnumerable<StatementSyntax> ErrorHandlingStatements()
    {
        var errorHandler = Execution.Model.ErrorHandler;
        if (errorHandler is null) {
            // builder.DefaultErrorHandler()
            // break;
            yield return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(ErrorsBuilder_VarIdentifier()),
                        SyntaxFactory.IdentifierName(Literals.ArgParsingErrorsBuilder_DefaultErrorHandler_MethodIdentifier))));
            yield return SyntaxFactory.BreakStatement();
        }
        else {
            var firstArgExpr = SyntaxProvider.SimpleMethodInvocation(
                SyntaxFactory.IdentifierName(ErrorsBuilder_VarIdentifier()),
                SyntaxFactory.IdentifierName(Literals.ArgParsingErrorsBuilder_Build_MethodIdentifier),
                SyntaxFactory.IdentifierName(ArgsProvider_VarIdentifier()));

            var invocation = SyntaxFactory.InvocationExpression(
                SyntaxProvider.SiblingMemberAccessExpression(errorHandler),
                SyntaxProvider.ArgumentList(
                    errorHandler.Parameters.Length switch {
                    1 => [firstArgExpr],
                    2 => [firstArgExpr, SyntaxProvider.LiteralStringExpression(Model.Symbol.Name)],
                    _ => throw new InvalidOperationException(),
                }));

            if (errorHandler.ReturnsVoid) {
                // ErrorHandler();
                // break;
                yield return SyntaxFactory.ExpressionStatement(invocation);
                yield return SyntaxFactory.BreakStatement();
            }
            else {
                // return ErrorHandler();
                yield return SyntaxFactory.ReturnStatement(invocation);
            }
        }
    }

    public Optional<FieldDeclarationSyntax> ParameterSetFieldDeclaration()
    {
        if (Parameters.Length == 0)
            return default;

        var namedParameters = Parameters
            .WhereSelect(p => WrapperUtils.OptionalNotNull(p.ParameterData as INamedParameterDataProvider))
            .ToList();

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
                        SyntaxFactory.Identifier(ParameterSet_FieldIdentifier()),
                        default,
                        SyntaxFactory.EqualsValueClause(
                            SyntaxFactory.ImplicitObjectCreationExpression(
                                SyntaxProvider.ArgumentList(
                                    DictParameterArgExpression(
                                        namedParameters,
                                        ImmutableArray.Create<TypeSyntax>(
                                            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                                            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword))),
                                        p => (SyntaxProvider.LiteralStringExpression(p.Data.Name),
                                            p.GetParameterSetDictValue())),
                                    DictParameterArgExpression(
                                        namedParameters.Where(p => p.Data.Alias is not null).ToList(),
                                        ImmutableArray.Create<TypeSyntax>(
                                            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                                            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword))),
                                        p => (SyntaxProvider.LiteralStringExpression(p.Data.Alias!),
                                            SyntaxProvider.LiteralStringExpression(p.Data.Name)))),
                                default))))));

        static ExpressionSyntax DictParameterArgExpression(IReadOnlyCollection<INamedParameterDataProvider> providers, ImmutableArray<TypeSyntax> dictTypeArguments, Func<INamedParameterDataProvider, (ExpressionSyntax Key, ExpressionSyntax Value)> initializerExpressionKeyValueSelector)
        {
            var dictType = SyntaxFactory.GenericName(
                SyntaxFactory.Identifier($"{Constants.Global}::{Constants.Dictionary_TypeName}"),
                SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SeparatedList(dictTypeArguments)));

            if (providers.Count == 0) {
                return SyntaxFactory.DefaultExpression(dictType);
            }

            return SyntaxFactory.ObjectCreationExpression(
                dictType,
                SyntaxProvider.ArgumentList(
                    SyntaxProvider.LiteralInt32Expression(providers.Count)),
                SyntaxFactory.InitializerExpression(
                    SyntaxKind.ObjectInitializerExpression,
                    SyntaxFactory.SeparatedList<ExpressionSyntax>(
                        providers.Select(p =>
                        {
                            var (key, value) = initializerExpressionKeyValueSelector(p);
                            return SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                SyntaxFactory.ImplicitElementAccess(
                                    SyntaxFactory.BracketedArgumentList(
                                        SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.Argument(key)))),
                                value);
                        }))));
        }
    }
}
