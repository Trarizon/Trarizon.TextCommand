using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
using Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.Parameters;
using Trarizon.TextCommand.SourceGenerator.Core.Tags;
using Trarizon.TextCommand.SourceGenerator.Utilities;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.ParameterDatas;
/// <param name="index">if &lt 0, means this value is after RestValues, thus always empty</param>
internal class MultiValueDataProvider(MultiValueParameterData data, InputParameterProvider parameter) : IInputParameterDataProvider, IRequiredParameterDataProvider, IValueDataProvider
{
    public MultiValueParameterData Data { get; } = data;

    public InputParameterProvider Parameter { get; } = parameter;

    IInputParameterData IInputParameterDataProvider.Data => Data;
    IRequiredParameterData IRequiredParameterDataProvider.Data => Data;
    IValueParameterData IValueDataProvider.Data => Data;
    IParameterData IParameterDataProvider.Data => Data;

    IParameterProvider IParameterDataProvider.Parameter => Parameter;

    public ProviderMethodInfoContext ProviderMethodInfo
    {
        get {
            const int BySpan = 0, ByArray = 1, ByList = 2;
            int flag = Data.CollectionType switch {
                MultiValueCollectionType.ReadOnlySpan or
                MultiValueCollectionType.Span
                    => Data.MaxCount <= Literals.StackAllocThreshold && Data.ResultTypeSymbol.IsUnmanagedType
                    && Data.ResultTypeSymbol.NullableAnnotation is NullableAnnotation.NotAnnotated // Nullable<T> cannot pass into unmanaged type param
                        ? BySpan : ByArray,
                MultiValueCollectionType.ReadOnlySpan or
                MultiValueCollectionType.Span or
                MultiValueCollectionType.Array or
                MultiValueCollectionType.Enumerable
                    => ByArray,
                MultiValueCollectionType.List
                    => ByList,
                _ => throw new InvalidOperationException(),
            };

            (string MethodIdentifier, TypeSyntax Arg2Type) item = flag switch {
                BySpan => (Literals.ArgsProvider_GetValuesUnmanaged_MethodIdentifier,
                    SyntaxFactory.GenericName(
                        SyntaxFactory.Identifier($"{Constants.Global}::{Literals.ArgResult_TypeName}"),
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                Parameter.ResultTypeSyntax)))),
                ByArray => (Literals.ArgsProvider_GetValuesArray_MethodIdentifier,
                    SyntaxFactory.IdentifierName($"{Constants.Global}::{Literals.ArgRawResultInfo_TypeName}")),
                ByList => (Literals.ArgsProvider_GetValuesList_MethodIdentifier,
                    SyntaxFactory.IdentifierName($"{Constants.Global}::{Literals.ArgRawResultInfo_TypeName}")),
                _ => throw new InvalidOperationException(),
            };

            return new ProviderMethodInfoContext(item.MethodIdentifier,
                [
                    SyntaxProvider.LiteralInt32Expression(Data.IsUnreachable ? int.MaxValue : Data.Index),
                    Parameter.ParserArgExpressionSyntax,
                    Data.IsUnreachable
                        ? SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.GenericName(
                                SyntaxFactory.Identifier($"{Constants.Global}::{Constants.Span_TypeName}"),
                                SyntaxFactory.TypeArgumentList(
                                    SyntaxFactory.SingletonSeparatedList(
                                        item.Arg2Type))),
                            SyntaxFactory.IdentifierName(Constants.Empty_Identifier))
                        : SyntaxFactory.StackAllocArrayCreationExpression(
                            SyntaxFactory.ArrayType(
                                item.Arg2Type,
                                SyntaxFactory.SingletonList(
                                    SyntaxFactory.ArrayRankSpecifier(
                                        SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                            GetAvailableArrayLengthInvocation())))))
                ]);
        }
    }

    private InvocationExpressionSyntax GetAvailableArrayLengthInvocation()
    {
        Debug.Assert(!Data.IsUnreachable);
        return SyntaxProvider.SimpleMethodInvocation(
            SyntaxFactory.IdentifierName(Parameter.Executor.ArgsProvider_VarIdentifier()),
            SyntaxFactory.IdentifierName(Literals.ArgsProvider_GetAvailableArrayLength_MethodIdentifier),
            SyntaxProvider.LiteralInt32Expression(Data.Index),
            SyntaxProvider.LiteralInt32Expression(Data.MaxCount));
    }

    public ExpressionSyntax ResultValueAccessExpression()
        => SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.IdentifierName(Parameter.Argument_VarIdentifier()),
            SyntaxFactory.IdentifierName(Literals.ArgResults_Values_PropertyIdentifier));
}
