using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas.Markers;
using Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.Parameters.Markers;
using Trarizon.TextCommand.SourceGenerator.Utilities;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.Parameters;
internal sealed class MultiValueParameterProvider : IInputParameterProvider, IMultipleParameterProvider, IRequiredParameterProvider, IPositionalParameterProvider
{
    private readonly InputParameterProviderComponent _helper;

    public MultiValueParameterData Data { get; }

    public ExecutorProvider Executor { get; }

    IParameterData IParameterProvider.Data => Data;
    IInputParameterData IInputParameterProvider.Data => Data;
    IRequiredParameterData IRequiredParameterProvider.Data => Data;
    IPositionalParameterData IPositionalParameterProvider.Data => Data;

    public MultiValueParameterProvider(MultiValueParameterData data, ExecutorProvider executor)
    {
        Data = data;
        Executor = executor;
        _helper = new(this);
    }

    public IEnumerable<StatementSyntax> CaseBodyLocalStatements()
    {
        yield return LocalVarDeclaration();

        if (!Data.IsUnreachable) {
            yield return _helper.StdErrorHandingStatement(Data.IsRequired);
        }
    }

    private StatementSyntax LocalVarDeclaration()
    {
        const int BySpan = 0, ByArray = 1, ByList = 2;
        int kind = GetMethodKind();

        string methodIdentifier = kind switch {
            BySpan => Literals.ArgsProvider_GetValuesUnmanaged_MethodIdentifier,
            ByArray => Literals.ArgsProvider_GetValuesArray_MethodIdentifier,
            ByList => Literals.ArgsProvider_GetValuesList_MethodIdentifier,
            _ => throw new NotImplementedException(),
        };

        var methodArgs = new ArgumentSyntax[3];
        methodArgs[0] = SyntaxFactory.Argument(
            SyntaxProvider.LiteralInt32Expression(Data.IsUnreachable ? int.MaxValue : Data.StartIndex));
        methodArgs[1] = SyntaxFactory.Argument(_helper.ParserArgExprSyntax);
        if (Data.IsUnreachable) {
            methodArgs[2] = SyntaxFactory.Argument(
                SyntaxFactory.CollectionExpression());
        }
        else {
            // stackalloc SpaceHolderType[_provider.GetArrayLength(start,max)]
            methodArgs[2] = SyntaxFactory.Argument(
                SyntaxFactory.StackAllocArrayCreationExpression(
                    SyntaxFactory.ArrayType(
                        GetSpaceHolderType(kind),
                        SyntaxFactory.SingletonList(
                            SyntaxFactory.ArrayRankSpecifier(
                                SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                    SyntaxProvider.SimpleMethodInvocation(
                                        SyntaxFactory.IdentifierName(Executor.L_ArgsProvider_VarIdentifier()),
                                        SyntaxFactory.IdentifierName(Literals.ArgsProvider_GetAvailableArrayLength_MethodIdentifier),
                                        SyntaxProvider.LiteralInt32Expression(Data.StartIndex),
                                        SyntaxProvider.LiteralInt32Expression(Data.MaxCount))))))));
        }

        return _helper.StdLocalVarDeclaration(methodIdentifier, methodArgs);

        int GetMethodKind()
        {
            switch (Data.CollectionKind) {
                case Tags.MultiParameterCollectionKind.ReadOnlySpan:
                case Tags.MultiParameterCollectionKind.Span:
                    bool useSpan = Data.MaxCount <= Literals.G_StackAllocThreshold
                        && Data.TargetElementTypeSymbol.IsUnmanagedType;
                    return useSpan ? BySpan : ByArray;
                case Tags.MultiParameterCollectionKind.Array:
                case Tags.MultiParameterCollectionKind.Enumerable:
                    return ByArray;
                case Tags.MultiParameterCollectionKind.List:
                    return ByList;
                default:
                    throw new InvalidOperationException();
            }
        }

        TypeSyntax GetSpaceHolderType(int kind)
        {
            switch (kind) {
                case BySpan: {
                    return SyntaxFactory.GenericName(
                        SyntaxFactory.Identifier($"{Constants.Global}::{Literals.ArgResult_TypeName}"),
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                _helper.ParserTargetTypeSyntax)));
                }
                case ByArray:
                case ByList:
                    return SyntaxFactory.IdentifierName($"{Constants.Global}::{Literals.ArgRawResultInfo_TypeName}");
                default:
                    throw new InvalidOperationException();
            }
        }
    }

    public ArgumentSyntax ExecutorArgAccess_ArgumentSyntax()
        => SyntaxFactory.Argument(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName(_helper.L_ExecutorArgument_VarIdentifier()),
                SyntaxFactory.IdentifierName(Literals.ArgResults_Values_PropertyIdentifier)));
}
