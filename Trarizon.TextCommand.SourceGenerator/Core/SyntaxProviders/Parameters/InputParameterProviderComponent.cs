using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas.Markers;
using Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.Parameters.Markers;
using Trarizon.TextCommand.SourceGenerator.Core.Tags;
using Trarizon.TextCommand.SourceGenerator.Utilities;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.Parameters;
internal sealed class InputParameterProviderComponent(IInputParameterProvider parameter)
{
    private readonly IInputParameterProvider _parameter = parameter;

    private TypeSyntax? _parserTargetTypeSyntax;
    public TypeSyntax ParserTargetTypeSyntax
    {
        get {
            if (_parserTargetTypeSyntax is null) {
                InitializeParserSyntax();
            }
            return _parserTargetTypeSyntax;
        }
    }

    private TypeSyntax? _parserTypeSyntax;
    public TypeSyntax ParserTypeSyntax
    {
        get {
            if (_parserTypeSyntax is null)
                InitializeParserSyntax();
            return _parserTypeSyntax;
        }
    }

    private ExpressionSyntax? _parserArgExprSyntax;
    public ExpressionSyntax ParserArgExprSyntax
    {
        get {
            if (_parserArgExprSyntax is null)
                InitializeParserSyntax();
            return _parserArgExprSyntax;
        }
    }

    [MemberNotNull(nameof(_parserTypeSyntax), nameof(_parserArgExprSyntax), nameof(_parserTargetTypeSyntax))]
    private void InitializeParserSyntax()
    {
        var parserInfo = _parameter.Data.ParserInfo;
        switch (parserInfo.Kind) {
            case CustomParserKind.Implicit: {
                _parserTypeSyntax = GetNonWrappedDefaultParserTypeSyntax(parserInfo);
                _parserArgExprSyntax = SyntaxFactory.DefaultExpression(_parserTypeSyntax);
                break;
            }
            case CustomParserKind.Field: {
                _parserTypeSyntax = SyntaxFactory.IdentifierName(parserInfo.Field.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                _parserArgExprSyntax = SyntaxProvider.SiblingMemberAccessExpression(parserInfo.Field);
                break;
            }
            case CustomParserKind.Property: {
                _parserTypeSyntax = SyntaxFactory.IdentifierName(parserInfo.Property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                _parserArgExprSyntax = SyntaxProvider.SiblingMemberAccessExpression(parserInfo.Property);
                break;
            }
            case CustomParserKind.Method: {
                string? identifier = parserInfo.Method.InputParameterKind switch {
                    MethodParserInputParameterKind.Flag => $"{Constants.Global}::{Literals.DelegateFlagParser_TypeName}",
                    MethodParserInputParameterKind.InputArg => $"{Constants.Global}::{Literals.DelegateParser_TypeName}",
                    MethodParserInputParameterKind.String => $"{Constants.Global}::{Literals.DelegateStringParser_TypeName}",
                    MethodParserInputParameterKind.ReadOnlySpan_Char => $"{Constants.Global}::{Literals.DelegateSpanParser_TypeName}",
                    _ => throw new InvalidOperationException(),
                };
                _parserTypeSyntax = SyntaxFactory.GenericName(
                    SyntaxFactory.Identifier(identifier),
                    SyntaxFactory.TypeArgumentList(
                        SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                            SyntaxFactory.IdentifierName(parserInfo.ParserReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)))));
                _parserArgExprSyntax = SyntaxFactory.ObjectCreationExpression(
                    _parserTypeSyntax,
                    SyntaxProvider.ArgumentList(
                        SyntaxProvider.SiblingMemberAccessExpression(parserInfo.Method.Symbol)),
                    initializer: default);
                break;
            }
            case CustomParserKind.Struct: {
                _parserTypeSyntax = SyntaxFactory.IdentifierName(parserInfo.Struct.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                _parserArgExprSyntax = SyntaxFactory.DefaultExpression(_parserTypeSyntax);
                break;
            }
            default:
                throw new InvalidOperationException();
        }

        WrapParserTypeSyntaxAndGetTargetType(out _parserTargetTypeSyntax);

        static TypeSyntax GetNonWrappedDefaultParserTypeSyntax(in CustomParserInfo parserInfo)
        {
            switch (parserInfo.ImplicitKind) {
                case ImplicitExecutorParameterKind.Boolean:
                    return SyntaxFactory.IdentifierName($"{Constants.Global}::{Literals.BooleanFlagParser_TypeName}");
                case ImplicitExecutorParameterKind.ISpanParsable:
                    return SyntaxFactory.GenericName(
                        SyntaxFactory.Identifier($"{Constants.Global}::{Literals.ParsableParser_TypeName}"),
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                SyntaxFactory.IdentifierName(parserInfo.ParserReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)))));
                case ImplicitExecutorParameterKind.Enum:
                    return SyntaxFactory.GenericName(
                        SyntaxFactory.Identifier($"{Constants.Global}::{Literals.EnumParser_TypeName}"),
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                SyntaxFactory.IdentifierName(parserInfo.ParserReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)))));
                default:
                    throw new InvalidOperationException();
            }
        }

        void WrapParserTypeSyntaxAndGetTargetType(out TypeSyntax parserTargetTypeSyntax)
        {
            // GetFlag<> directly returns value, so we doesn't need wrap
            parserTargetTypeSyntax = SyntaxProvider.FullQualifiedIdentifierName(_parameter.Data.ParserInfo.ParserReturnType);
            if (_parameter is IFlagParameterProvider) {
                return;
            }

            var kinds = _parameter.Data.GetParserWrapperKinds();
            if (kinds is ParserWrapperKinds.None) {
                return;
            }

            if (kinds.HasFlag(ParserWrapperKinds.Nullable)) {
                //parserTargetTypeSyntax = SyntaxFactory.NullableType(parserTargetTypeSyntax);

                _parserTypeSyntax = SyntaxFactory.GenericName(
                    SyntaxFactory.Identifier($"{Constants.Global}::{Literals.NullableParser_TypeName}"),
                    SyntaxFactory.TypeArgumentList(
                        SyntaxFactory.SeparatedList(new TypeSyntax[] {
                            parserTargetTypeSyntax,
                            _parserTypeSyntax,
                        })));
                if (_parserArgExprSyntax is DefaultExpressionSyntax) {
                    _parserArgExprSyntax = SyntaxFactory.DefaultExpression(_parserTypeSyntax);
                }
                else {
                    _parserArgExprSyntax = SyntaxFactory.ObjectCreationExpression(
                        _parserTypeSyntax,
                        SyntaxProvider.ArgumentList(
                            _parserArgExprSyntax),
                        initializer: null);
                }

                parserTargetTypeSyntax = SyntaxFactory.NullableType(parserTargetTypeSyntax);
            }

            // For int -> From<int>?, we use NullableParser<> first,
            // and then wrap with Conversion<int?, From<int>?, >.
            // So we needn't wrap Conversion<> for non-multiple params
            if (kinds.HasFlag(ParserWrapperKinds.ImplicitConversion)) {
                if (_parameter is not IMultipleParameterProvider) {
                    return;
                }
                var fromType = parserTargetTypeSyntax;
                parserTargetTypeSyntax = SyntaxProvider.FullQualifiedIdentifierName(_parameter.Data.TargetElementTypeSymbol);

                _parserTypeSyntax = SyntaxFactory.GenericName(
                    SyntaxFactory.Identifier($"{Constants.Global}::{Literals.ConversionParser_TypeName}"),
                    SyntaxFactory.TypeArgumentList(
                        SyntaxFactory.SeparatedList(new TypeSyntax[] {
                            fromType,
                            parserTargetTypeSyntax,
                            _parserTypeSyntax,
                        })));
                _parserArgExprSyntax = SyntaxFactory.ObjectCreationExpression(
                    _parserTypeSyntax,
                    SyntaxProvider.ArgumentList(
                        _parserArgExprSyntax,
                        SyntaxFactory.SimpleLambdaExpression(
                            SyntaxFactory.Parameter(
                                SyntaxFactory.Identifier("x")),
                            block: null,
                            SyntaxFactory.IdentifierName("x"))),
                    initializer: null);

            }

            // After check None, at least one of the if statement will be matched.
        }
    }

    public string L_ExecutorArgument_VarIdentifier() => $"{Literals.G_ExecutorArgument_VarIdentifier}_{_parameter.Executor.Model.Symbol.Name}_{_parameter.Data.Model.Symbol.Name}";

    public LocalDeclarationStatementSyntax StdLocalVarDeclaration(string argProvider_Get_MethodIdentifier, IEnumerable<ArgumentSyntax> methodArgs)
    {
        return SyntaxProvider.LocalVarSingleVariableDeclaration(
            L_ExecutorArgument_VarIdentifier(),
            SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(_parameter.Executor.L_ArgsProvider_VarIdentifier()),
                    SyntaxFactory.GenericName(
                        SyntaxFactory.Identifier(argProvider_Get_MethodIdentifier),
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SeparatedList(new[] {
                                ParserTargetTypeSyntax,
                                ParserTypeSyntax,
                            })))),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(methodArgs))));
    }

    public ExpressionStatementSyntax StdErrorHandingStatement(bool isRequired)
    {
        string argResultKind = isRequired
            ? Literals.ArgResultKind_ParameterNotSet_FieldName
            : Literals.ArgResultKind_ParsingFailed_FieldName;

        // errorBuilder.AddWhenError();
        return SyntaxFactory.ExpressionStatement(
            SyntaxProvider.SimpleMethodInvocation(
                self: SyntaxFactory.IdentifierName(_parameter.Executor.L_ErrorsBuilder_VarIdentifier()),
                method: SyntaxFactory.IdentifierName(Literals.ArgParsingErrorsBuilder_AddWhenError_MethodIdentifier),
                SyntaxFactory.IdentifierName(L_ExecutorArgument_VarIdentifier()),
                SyntaxProvider.LiteralStringExpression(_parameter.Data.Model.Symbol.Name),
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName($"{Constants.Global}::{Literals.ArgResultKind_TypeName}"),
                    SyntaxFactory.IdentifierName(argResultKind))));
    }
}
