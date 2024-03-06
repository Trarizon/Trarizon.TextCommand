using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
            null or _ => throw new InvalidOperationException(),
        };
        Executor = executor;

    }

    public string Argument_VarIdentifier() => $"__arg_{Model.Symbol.Name}_{Executor.Model.Symbol.Name}";

    private TypeSyntax? _resultTypeSyntax;
    public TypeSyntax ResultTypeSyntax => _resultTypeSyntax ??= SyntaxFactory.IdentifierName(ParameterData.Data.ResultTypeSymbol.ToDisplayString(SymbolDisplayFormats.FullQualifiedFormatIncludeNullableRefTypeModifier));

    private TypeSyntax? _parsedTypeSyntax;
    public TypeSyntax ParsedTypeSyntax => _parsedTypeSyntax ??= SyntaxFactory.IdentifierName(ParameterData.Data.ParsedTypeSymbol.ToDisplayString(SymbolDisplayFormats.FullQualifiedFormatIncludeNullableRefTypeModifier));

    private TypeSyntax? _parserTypeSyntax;
    public TypeSyntax ParserTypeSyntax
    {
        get {
            if (_parserTypeSyntax is null) {
                InitializeParserSyntax();
            }
            return _parserTypeSyntax;
        }
    }

    private ExpressionSyntax? _parserArgExpressionSyntax;
    public ExpressionSyntax ParserArgExpressionSyntax
    {
        get {
            if (_parserArgExpressionSyntax is null) {
                InitializeParserSyntax();
            }
            return _parserArgExpressionSyntax;
        }
    }

    [MemberNotNull(nameof(_parserTypeSyntax), nameof(_parserArgExpressionSyntax))]
    private void InitializeParserSyntax()
    {
        var parserInfo = ParameterData.Data.ParserInfo;
        switch (parserInfo.Kind) {
            case ParserInfoProvider.ParserKind.Implicit:
                _parserTypeSyntax = SyntaxHelper.GetNonWrappedDefaultParserTypeSyntax(ParameterData.Data);
                _parserArgExpressionSyntax = SyntaxFactory.DefaultExpression(ParserTypeSyntax);
                break;

            case ParserInfoProvider.ParserKind.FieldOrProperty:
                (var type, var member) = parserInfo.FieldOrProperty;
                _parserTypeSyntax = SyntaxFactory.IdentifierName(type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                _parserArgExpressionSyntax = SyntaxProvider.SiblingMemberAccessExpression(member);
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
                _parserArgExpressionSyntax = SyntaxFactory.ObjectCreationExpression(
                     ParserTypeSyntax,
                     SyntaxProvider.ArgumentList(
                         SyntaxProvider.SiblingMemberAccessExpression(parserInfo.Method)),
                     default);
                break;

            case ParserInfoProvider.ParserKind.Struct:
                _parserTypeSyntax = SyntaxFactory.IdentifierName(parserInfo.Struct.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                _parserArgExpressionSyntax = SyntaxFactory.DefaultExpression(ParserTypeSyntax);
                break;
            default:
                throw new InvalidOperationException();
        }
        (_parserTypeSyntax, _parserArgExpressionSyntax) = SyntaxHelper.WrapParserTypeSyntax(_parserTypeSyntax, _parserArgExpressionSyntax, ParameterData.Data);
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
                            ResultTypeSyntax,
                            ParserTypeSyntax,
                        }))),
                context.ArgExpressions));
    }

    public IEnumerable<StatementSyntax> ArgumentLocalExtraStatments()
    {
        if (ErrorHandlingExtraStatement().TryGetValue(out var errHan)) {
            yield return errHan;
        }
    }

    private Optional<StatementSyntax> ErrorHandlingExtraStatement()
    {
        // Flag Getter returns raw bool value
        if (ParameterData is FlagDataProvider) {
            return default;
        }
        // Unreachable value doesn't need to check error
        if (ParameterData is IValueDataProvider { Data.IsUnreachable: true }) {
            return default;
        }

        // 目前情况，除了Flag以外的都实现IRequiredProvider，也就是说下面的if必定判true，
        // 暂时没有想到interface怎么设计，所以就先这样了。不增加参数种类情况下这里没问题。

        string argResultKind;
        if (ParameterData is IRequiredParameterDataProvider requiredParameter) {
            argResultKind = requiredParameter.Data.Required
                ? Literals.ArgResultKind_ParameterNotSet_FieldName
                : Literals.ArgResultKind_ParsingFailed_FieldName;
        }
        else {
            argResultKind = Literals.ArgResultKind_ParsingFailed_FieldName;
        }

        // errorBuilder.AddWhenError()
        return SyntaxFactory.ExpressionStatement(
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
