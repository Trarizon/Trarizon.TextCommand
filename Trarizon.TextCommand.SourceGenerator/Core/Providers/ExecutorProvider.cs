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

    public string RestArgs_VarIdentifier => $"__rest_{Model.Symbol.Name}";
    public string ArgsProvider_VarIdentifer => $"__provider_{Model.Symbol.Name}";
    public string ParameterSet_FieldIdentifier => Model.Symbol.Name;

    public SwitchSectionSyntax MatchingSwitchSection()
    {
        var casePatternLabel = SyntaxFactory.CasePatternSwitchLabel(
            SyntaxFactory.ListPattern(
                SyntaxFactory.SeparatedList(
                    ListPatternSubPatterns())),
            SyntaxFactory.Token(SyntaxKind.ColonToken));

        return SyntaxFactory.SwitchSection(
            SyntaxFactory.SingletonList<SwitchLabelSyntax>(
                casePatternLabel),
            SyntaxFactory.List(
                CaseStatements()));
    }

    private IEnumerable<PatternSyntax> ListPatternSubPatterns()
    {
        if (Execution.CommandPrefix is not null) {
            yield return SyntaxFactory.ConstantPattern(
                SyntaxProvider.LiteralStringExpression(Execution.CommandPrefix));
        }

        foreach (var prefix in Model.CommandPrefixes) {
            yield return SyntaxFactory.ConstantPattern(
                SyntaxProvider.LiteralStringExpression(prefix));
        }

        if (Parameters.Length > 0) {
            yield return SyntaxFactory.SlicePattern(
                SyntaxFactory.VarPattern(
                    SyntaxFactory.SingleVariableDesignation(
                        SyntaxFactory.Identifier(RestArgs_VarIdentifier))));
        }
        else {
            yield return SyntaxFactory.SlicePattern();
        }
    }

    private IEnumerable<StatementSyntax> CaseStatements()
    {
        if (Parameters.Length > 0) {
            // __provider = ParamSet.Exec.Parser(__rest)
            yield return SyntaxProvider.LocalVarSingleVariableDeclaration(
                ArgsProvider_VarIdentifer,
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.IdentifierName(Constants.Global($"{Literals.ParameterSets_TypeIdentifier}.{ParameterSet_FieldIdentifier}.{Literals.ParameterSet_Parse_MethodIdentifier}")),
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Argument(
                                SyntaxFactory.IdentifierName(RestArgs_VarIdentifier))))));

            foreach (var parameter in Parameters) {
                // var __prm_exec = Get<>();
                yield return parameter.ArgumentLocalDeclaration();
            }
        }

        // return Exec(__prm_exec, ..);
        yield return SyntaxFactory.ReturnStatement(
            SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    Model.Symbol.IsStatic
                        ? SyntaxFactory.IdentifierName(Execution.Command.Symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
                        : SyntaxFactory.ThisExpression(),
                    SyntaxFactory.IdentifierName(Model.Symbol.Name)),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(
                        Parameters.Select(p
                            => SyntaxFactory.Argument(
                                SyntaxFactory.IdentifierName(
                                    p.Argument_VarIdentifier)))))));
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
                SyntaxFactory.IdentifierName(Constants.Global(Literals.ParameterSet_TypeName)),
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
                                                SyntaxFactory.IdentifierName(Constants.Global(Constants.String_TypeName)),
                                                SyntaxFactory.IdentifierName(Constants.Global(Constants.Boolean_TypeName)),
                                            ],
                                            p => (SyntaxProvider.LiteralStringExpression(Literals.FullName(p.Name)),
                                                SyntaxProvider.LiteralBooleanExpression(p is OptionProvider))),
                                        DictParameterArgument(
                                            aliasParameters,
                                            [
                                                SyntaxFactory.IdentifierName(Constants.Global(Constants.String_TypeName)),
                                                SyntaxFactory.IdentifierName(Constants.Global(Constants.String_TypeName)),
                                            ],
                                            p => (SyntaxProvider.LiteralStringExpression(Literals.Alias(p.Alias!)),
                                                SyntaxProvider.LiteralStringExpression(Literals.FullName(p.Name)))),
                                    })),
                                default))))));


        static ArgumentSyntax DictParameterArgument(List<INamedParameterProvider> parameters, TypeSyntax[] dictTypeArguments, Func<INamedParameterProvider, (ExpressionSyntax Key, ExpressionSyntax Value)> initializerExpressionKeyValueSelector)
        {
            var dictType = SyntaxFactory.GenericName(
                SyntaxFactory.Identifier(Constants.Global(Constants.Dictionary_TypeName)),
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
