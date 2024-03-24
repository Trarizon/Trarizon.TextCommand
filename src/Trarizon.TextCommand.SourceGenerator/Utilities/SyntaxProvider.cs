using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;

namespace Trarizon.TextCommand.SourceGenerator.Utilities;
internal static class SyntaxProvider
{
    public static readonly AttributeListSyntax GeneratedCodeAttributeSyntax =
        SyntaxFactory.AttributeList(
            SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Attribute(
                    SyntaxFactory.ParseName(
                        $"{Constants.Global}::{Constants.GeneratedCodeAttribute_TypeName}"),
                    SyntaxFactory.AttributeArgumentList(
                        SyntaxFactory.SeparatedList(new[] {
                            SyntaxFactory.AttributeArgument(
                                LiteralStringExpression(Literals.GeneratorNamespace)),
                            SyntaxFactory.AttributeArgument(
                                LiteralStringExpression(Literals.Version)),
                        })))));

    #region SyntaxFactory Sugar

    public static IdentifierNameSyntax FullQualifiedIdentifierName(ISymbol symbol)
    {
        return SyntaxFactory.IdentifierName(symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
    }

    public static SyntaxToken RefKindToken(RefKind parameterRefKind)
    {
        return SyntaxFactory.Token(
            parameterRefKind switch {
                RefKind.None => SyntaxKind.None,
                RefKind.Ref => SyntaxKind.RefKeyword,
                RefKind.Out => SyntaxKind.OutKeyword,
                RefKind.In => SyntaxKind.InKeyword,
                RefKind.RefReadOnlyParameter => SyntaxKind.InKeyword,
                _ => throw new System.InvalidOperationException(),
            });
    }

    public static MemberAccessExpressionSyntax SiblingMemberAccessExpression(ISymbol member)
    {
        return SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            member.IsStatic
                ? SyntaxFactory.IdentifierName(member.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
                : SyntaxFactory.ThisExpression(),
            SyntaxFactory.IdentifierName(member.Name));
    }

    public static LocalDeclarationStatementSyntax LocalVarSingleVariableDeclaration(string variableName, ExpressionSyntax initializer)
        => SyntaxFactory.LocalDeclarationStatement(
            SyntaxFactory.VariableDeclaration(
                SyntaxFactory.IdentifierName("var"),
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.VariableDeclarator(
                        SyntaxFactory.Identifier(variableName),
                        default,
                        SyntaxFactory.EqualsValueClause(initializer)))));

    public static LiteralExpressionSyntax LiteralDefaultExpression()
        => SyntaxFactory.LiteralExpression(
            SyntaxKind.DefaultLiteralExpression);

    public static LiteralExpressionSyntax LiteralStringExpression(string str)
        => SyntaxFactory.LiteralExpression(
            SyntaxKind.StringLiteralExpression,
            SyntaxFactory.Literal(str));

    public static LiteralExpressionSyntax LiteralInt32Expression(int value)
        => SyntaxFactory.LiteralExpression(
            SyntaxKind.NumericLiteralExpression,
            SyntaxFactory.Literal(value));

    public static InvocationExpressionSyntax SimpleMethodInvocation(ExpressionSyntax self, SimpleNameSyntax method, ExpressionSyntax arg)
    {
        return SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                self,
                method),
            ArgumentList(arg));
    }

    public static InvocationExpressionSyntax SimpleMethodInvocation(ExpressionSyntax self, SimpleNameSyntax method, params ExpressionSyntax[] args)
        => SimpleMethodInvocation(self, method, args.AsEnumerable());

    public static InvocationExpressionSyntax SimpleMethodInvocation(ExpressionSyntax self, SimpleNameSyntax method, IEnumerable<ExpressionSyntax> args)
    {
        return SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                self,
                method),
            ArgumentList(args));
    }

    public static ArgumentListSyntax ArgumentList(ExpressionSyntax arg)
        => SyntaxFactory.ArgumentList(
            SyntaxFactory.SingletonSeparatedList(
               SyntaxFactory.Argument(arg)));

    public static ArgumentListSyntax ArgumentList(params ExpressionSyntax[] args)
        => SyntaxFactory.ArgumentList(
            SyntaxFactory.SeparatedList(
                args.Select(SyntaxFactory.Argument)));

    public static ArgumentListSyntax ArgumentList(IEnumerable<ExpressionSyntax> args)
        => SyntaxFactory.ArgumentList(
            SyntaxFactory.SeparatedList(
                args.Select(SyntaxFactory.Argument)));

    #endregion
}
