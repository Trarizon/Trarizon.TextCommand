using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
using Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.ParameterDatas;
using Trarizon.TextCommand.SourceGenerator.Utilities;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders;
internal class ParameterProvider
{
    public ParameterModel Model { get; }

    public IParameterDataProvider ParameterData { get; }

    public ExecutorProvider Executor { get; }

    /// <remarks>
    /// May throw
    /// </remarks>
    public ParameterProvider(ParameterModel model, ExecutorProvider executor)
    {
        Model = model;
        ParameterData = model.ParameterData switch {
            FlagParameterData flag => new FlagDataProvider(flag, this),
            OptionParameterData option => new OptionDataProvider(option, this),
            ValueParameterData value => new ValueDataProvider(value, this),
            MultiValueParameterData multiVal => new MultiValueDataProvider(multiVal, this),
            _ => throw new InvalidOperationException(),
        };
        Executor = executor;

    }

    public string Argument_VarIdentifier() => $"__arg_{Model.Symbol.Name}_{Executor.Model.Symbol.Name}";

    private TypeSyntax? _parsedTypeSyntax;
    public TypeSyntax ParsedTypeSyntax => _parsedTypeSyntax ??= SyntaxFactory.IdentifierName(ParameterData.Data.ParsedTypeSymbol.ToDisplayString(SymbolDisplayFormats.FullQualifiedFormatIncludeNullableRefTypeModifier));

    private TypeSyntax? _parserTypeSyntax;
    public TypeSyntax ParserTypeSyntax
    {
        get {
            if (_parserTypeSyntax is null) {
                var parserInfo = ParameterData.Data.ParserInfo;
                switch (parserInfo.Kind) {
                    case ParserInfoProvider.ParserKind.Implicit:
                        _parserTypeSyntax = SyntaxHelper.GetDefaultParserType(ParameterData.Data.ParsedTypeSymbol, parserInfo.ImplicitParameterKind);
                        break;

                    case ParserInfoProvider.ParserKind.FieldOrProperty:
                        (var type, _) = parserInfo.FieldOrProperty;
                        _parserTypeSyntax = SyntaxFactory.IdentifierName(type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                        break;

                    case ParserInfoProvider.ParserKind.Method:
                        var identifier = Model.ParameterKind == ParameterKind.Flag
                            ? $"{Constants.Global}::{Literals.DelegateFlagParser_TypeName}"
                            : $"{Constants.Global}::{Literals.DelegateParser_TypeName}";
                        _parserTypeSyntax = SyntaxFactory.GenericName(
                            SyntaxFactory.Identifier(identifier),
                            SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SingletonSeparatedList(
                                    ParsedTypeSyntax)));
                        break;

                    case ParserInfoProvider.ParserKind.Struct:
                        _parserTypeSyntax = SyntaxFactory.IdentifierName(parserInfo.Struct.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            return _parserTypeSyntax;
        }
    }

    private ExpressionSyntax? _parserArgExpressionSyntax;
    public ExpressionSyntax ParserArgExpressionSyntax
    {
        get {
            if (_parserArgExpressionSyntax is null) {
                var parserInfo = ParameterData.Data.ParserInfo;
                switch (parserInfo.Kind) {
                    case ParserInfoProvider.ParserKind.Implicit:
                    case ParserInfoProvider.ParserKind.Struct:
                        _parserArgExpressionSyntax = SyntaxFactory.DefaultExpression(ParserTypeSyntax);
                        break;

                    case ParserInfoProvider.ParserKind.FieldOrProperty:
                        (_, var member) = parserInfo.FieldOrProperty;
                        _parserArgExpressionSyntax = SyntaxProvider.SiblingMemberAccessExpression(member);
                        break;

                    case ParserInfoProvider.ParserKind.Method:
                        _parserArgExpressionSyntax = SyntaxFactory.ObjectCreationExpression(
                                ParserTypeSyntax,
                                SyntaxProvider.ArgumentList(
                                    SyntaxProvider.SiblingMemberAccessExpression(parserInfo.Method)),
                                default);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            return _parserArgExpressionSyntax;
        }
    }

    public LocalDeclarationStatementSyntax ArgumentLocalDeclaration()
    {
        var context = ParameterData.ProviderMethodInfo;

        return SyntaxProvider.LocalVarSingleVariableDeclaration(
            Argument_VarIdentifier(),
            SyntaxProvider.SimpleMethodInvocation(
                SyntaxFactory.IdentifierName(Executor.ArgsProvider_VarIdentifier()),
                SyntaxFactory.GenericName(
                    SyntaxFactory.Identifier(context.GetterMethodIdentifier),
                    SyntaxFactory.TypeArgumentList(
                        SyntaxFactory.SeparatedList(new[] {
                            ParsedTypeSyntax,
                            ParserTypeSyntax,
                        }))),
                context.ArgExpressions));
    }

    public IEnumerable<StatementSyntax> ArgumentLocalExtraStatments()
    {
        if (ParameterData is FlagDataProvider) {
            yield break;
        }

        // 目前情况，除了Flag以外的都实现IRequiredProvider，也就是说下面的if必定判true，
        // 暂时没有想到interface怎么设计，所以就先这样了。不增加参数种类情况下这里没问题。

        string argResultKind;
        if (ParameterData is IRequiredParameterDataProvider requiredParameter) {
            argResultKind = requiredParameter.Data.Required
                ? Literals.ArgResultKind_ParsingFailed_FieldName
                : Literals.ArgResultKind_ParameterNotSet_FieldName;
        }
        else {
            argResultKind = Literals.ArgResultKind_ParameterNotSet_FieldName;
        }

        // errorBuilder.AddWhenError()
        yield return SyntaxFactory.ExpressionStatement(
            SyntaxProvider.SimpleMethodInvocation(
                SyntaxFactory.IdentifierName(Executor.ErrorsBuilder_VarIdentifier()),
                SyntaxFactory.IdentifierName(Literals.ArgParsingErrorsBuilder_AddWhenError_MethodIdentifier),
                SyntaxFactory.IdentifierName(Argument_VarIdentifier()),
                SyntaxProvider.LiteralStringExpression(Model.Symbol.Name),
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName($"{Constants.Global}::{Literals.ArgResultKind_TypeName}"),
                    SyntaxFactory.IdentifierName(argResultKind))));
    }
}
