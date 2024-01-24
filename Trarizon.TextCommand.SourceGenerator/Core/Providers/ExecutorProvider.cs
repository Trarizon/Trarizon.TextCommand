using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models;
using Trarizon.TextCommand.SourceGenerator.Core.Models.Parameters;
using Trarizon.TextCommand.SourceGenerator.Core.Providers.Parameters;
using Trarizon.TextCommand.SourceGenerator.Utilities;

namespace Trarizon.TextCommand.SourceGenerator.Core.Providers;
internal sealed class ExecutorProvider
{
    public ExecutorModel Model { get; }

    public ExecutionProvider Execution { get; }

    public ParameterProvider[] Parameters { get; }

    public ExecutorProvider(ExecutionProvider execution, ExecutorModel model)
    {
        Model = model;
        Execution = execution;
        Parameters = new ParameterProvider[model.Parameters.Length];
        int valueIndex = 0;
        for (int i = 0; i < Parameters.Length; i++) {
            Parameters[i] = model.Parameters[i].CLParameter switch {
                FlagParameterModel flg => new FlagProvider(this, flg),
                OptionParameterModel opt => new OptionProvider(this, opt),
                ValueParameterModel val => new ValueProvider(this, val, valueIndex++),
                MultiValueParameterModel mtv => new MultiValueProvider(this, mtv, GetAndPlus(ref valueIndex, mtv.MaxCount)),
                _ => throw new InvalidOperationException(),
            };
        }

        static int GetAndPlus(ref int value, int plus)
        {
            var rtn = value;
            value += plus;
            return rtn;
        }
    }

    // Syntaxes

    public string RestArgs_VarIdentifier(int? postfix) => $"__rest_{Model.Symbol.Name}{postfix}";
    public string ArgsProvider_VarIdentifer => $"__provider_{Model.Symbol.Name}";
    public string ParameterSet_FieldIdentifier => Model.Symbol.Name;
    public string RunExecutor_LabelIdentifier => $"__{Model.Symbol.Name}_RUN_EXECUTOR";

    public IEnumerable<SwitchSectionSyntax> MatchingSwitchSections()
    {
        // case [..]:
        // __Label:
        // return Executor();
        // case [..]:
        // goto ;
        if (Model.CommandPrefixes.Length > 1 && Parameters.Length > 0) {
            // More than 1 [ExecutorAttribute] and requires __rest_var,
            // we use goto to jump
            yield return SyntaxFactory.SwitchSection(
                SyntaxFactory.SingletonList<SwitchLabelSyntax>(
                    SyntaxFactory.CasePatternSwitchLabel(
                        SyntaxFactory.ListPattern(
                            SyntaxFactory.SeparatedList(
                                ListPatternSubPatterns(Model.CommandPrefixes[0]))),
                        SyntaxFactory.Token(SyntaxKind.ColonToken))),
                SyntaxFactory.List(
                    MainCaseStatements()));

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
        // case [..]:
        // case [..]:
        //     ...
        else {
            yield return SyntaxFactory.SwitchSection(
                    SyntaxFactory.List<SwitchLabelSyntax>(
                        Model.CommandPrefixes.Select(cmdPrefixes =>
                        {
                            return SyntaxFactory.CasePatternSwitchLabel(
                                SyntaxFactory.ListPattern(
                                    SyntaxFactory.SeparatedList(
                                        ListPatternSubPatterns(cmdPrefixes))),
                                SyntaxFactory.Token(SyntaxKind.ColonToken));
                        })),
                    SyntaxFactory.List(
                        MainCaseStatements()));
        }
    }

    private IEnumerable<PatternSyntax> ListPatternSubPatterns(string[] executorCommandPrefixes, int? restIdentifierPostfix = default)
    {
        if (Execution.CommandPrefix is not null) {
            yield return SyntaxFactory.ConstantPattern(
                SyntaxProvider.LiteralStringExpression(Execution.CommandPrefix));
        }

        foreach (var prefix in executorCommandPrefixes) {
            yield return SyntaxFactory.ConstantPattern(
                SyntaxProvider.LiteralStringExpression(prefix));
        }

        if (Parameters.Length > 0) {
            yield return SyntaxFactory.SlicePattern(
                SyntaxFactory.VarPattern(
                    SyntaxFactory.SingleVariableDesignation(
                        SyntaxFactory.Identifier(RestArgs_VarIdentifier(restIdentifierPostfix)))));
        }
        else {
            yield return SyntaxFactory.SlicePattern();
        }
    }

    private StatementSyntax ArgsProvider_LocalVarStatement(bool declaration, int? restIdentifierPostfix = default)
    {
        var invocation = SyntaxFactory.InvocationExpression(
            SyntaxFactory.IdentifierName($"{Constants.Global}::{Literals.ParameterSets_TypeIdentifier}.{ParameterSet_FieldIdentifier}.{Literals.ParameterSet_Parse_MethodIdentifier}"),
            SyntaxFactory.ArgumentList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Argument(
                        SyntaxFactory.IdentifierName(RestArgs_VarIdentifier(restIdentifierPostfix))))));

        if (declaration) {
            return SyntaxProvider.LocalVarSingleVariableDeclaration(
                ArgsProvider_VarIdentifer,
                invocation);
        }
        else {
            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(ArgsProvider_VarIdentifer),
                    invocation));
        }
    }

    private IEnumerable<StatementSyntax> SubCaseStatements(int? restIdentifierPostfix)
    {
        yield return ArgsProvider_LocalVarStatement(false, restIdentifierPostfix);
        yield return SyntaxFactory.GotoStatement(
            SyntaxKind.GotoStatement,
            SyntaxFactory.IdentifierName(RunExecutor_LabelIdentifier));
    }

    private IEnumerable<StatementSyntax> MainCaseStatements()
    {
        if (Parameters.Length > 0) {
            // __provider = ParamSet.Exec.Parser(__rest)
            yield return ArgsProvider_LocalVarStatement(true);

            yield return SyntaxFactory.LabeledStatement(
                RunExecutor_LabelIdentifier,
                Parameters[0].ArgumentLocalDeclaration());

            // var __prm_exec = Get<>();
            for (int i = 1; i < Parameters.Length; i++) {
                yield return Parameters[i].ArgumentLocalDeclaration();
            }
        }

        var invocation = SyntaxFactory.InvocationExpression(
            SyntaxProvider.SiblingMemberAccessExpression(Model.Symbol),
            SyntaxFactory.ArgumentList(
                SyntaxFactory.SeparatedList(
                    Parameters.Select(p
                        => SyntaxFactory.Argument(
                            SyntaxFactory.IdentifierName(
                                p.Argument_VarIdentifier))))));

        // return Exec(__prm_exec, ..);
        if (Execution.Symbol.ReturnsVoid) {
            yield return SyntaxFactory.ExpressionStatement(invocation);
            yield return SyntaxFactory.ReturnStatement();
        }
        else {
            yield return SyntaxFactory.ReturnStatement(invocation);
        }
    }

    public FieldDeclarationSyntax ParameterSet_FieldDeclaration()
    {
        List<INamedParameterProvider> namedParameters = [];
        List<INamedParameterProvider> aliasParameters = [];
        foreach (var parameter in Parameters) {
            if (parameter is INamedParameterProvider namedParameter) {
                namedParameters.Add(namedParameter);
                if (namedParameter.Alias != null) {
                    aliasParameters.Add(namedParameter);
                }
            }
        }

        // public static readonlys ParameterSet Field = new(..);
        return SyntaxFactory.FieldDeclaration(
            SyntaxFactory.SingletonList(
                SyntaxProvider.GeneratedCodeAttributeSyntax),
            SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.StaticKeyword),
                SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)),
            SyntaxFactory.VariableDeclaration(
                SyntaxFactory.IdentifierName($"{Constants.Global}::{Literals.ParameterSet_TypeName}"),
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.VariableDeclarator(
                        SyntaxFactory.Identifier(ParameterSet_FieldIdentifier),
                        default,
                        SyntaxFactory.EqualsValueClause(
                            SyntaxFactory.ImplicitObjectCreationExpression(
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SeparatedList(new[] {
                                        DictParameterArgument(
                                            namedParameters,
                                            [
                                                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                                                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                                            ],
                                            p => (SyntaxProvider.LiteralStringExpression($"{Literals.Prefix}{p.Name}"),
                                                SyntaxProvider.LiteralBooleanExpression(p is OptionProvider))),
                                        DictParameterArgument(
                                            aliasParameters,
                                            [
                                                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                                                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
                                            ],
                                            p => (SyntaxProvider.LiteralStringExpression($"{Literals.Prefix_Alias}{p.Alias!}"),
                                                SyntaxProvider.LiteralStringExpression($"{Literals.Prefix}{p.Name}"))),
                                    })),
                                default))))));


        static ArgumentSyntax DictParameterArgument(List<INamedParameterProvider> parameters, TypeSyntax[] dictTypeArguments, Func<INamedParameterProvider, (ExpressionSyntax Key, ExpressionSyntax Value)> initializerExpressionKeyValueSelector)
        {
            var dictType = SyntaxFactory.GenericName(
                SyntaxFactory.Identifier($"{Constants.Global}::{Constants.Dictionary_TypeName}"),
                SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SeparatedList(dictTypeArguments)));

            if (parameters.Count == 0) {
                return SyntaxFactory.Argument(
                    SyntaxFactory.DefaultExpression(dictType));
            }

            return SyntaxFactory.Argument(
                SyntaxFactory.ObjectCreationExpression(
                    dictType,
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Argument(
                                SyntaxProvider.LiteralInt32Expression(parameters.Count)))),
                    SyntaxFactory.InitializerExpression(
                        SyntaxKind.ObjectInitializerExpression,
                        SyntaxFactory.SeparatedList<ExpressionSyntax>(
                            parameters.Select(p =>
                            {
                                var (keyExpression, valueExpression) = initializerExpressionKeyValueSelector(p);
                                return SyntaxFactory.AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    SyntaxFactory.ImplicitElementAccess(
                                        SyntaxFactory.BracketedArgumentList(
                                            SyntaxFactory.SingletonSeparatedList(
                                                SyntaxFactory.Argument(keyExpression)))),
                                    valueExpression);
                            })))));
        }
    }
}
