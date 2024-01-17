using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;

namespace Trarizon.TextCommand.SourceGenerator.Utilities;
internal static class SyntaxProvider
{
    public static readonly AttributeListSyntax GeneratedCodeAttributeSyntax =
        SyntaxFactory.AttributeList(
            SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Attribute(
                    SyntaxFactory.ParseName(
                        Constants.Global(Constants.GeneratedCodeAttribute_TypeName)),
                    SyntaxFactory.AttributeArgumentList(
                        SyntaxFactory.SeparatedList(new[] {
                            SyntaxFactory.AttributeArgument(
                                LiteralStringExpression(Literals.GeneratorNamespace)),
                            SyntaxFactory.AttributeArgument(
                                LiteralStringExpression(Literals.Version)),
                        })))));

    #region SyntaxFactory Sugar

    public static MemberAccessExpressionSyntax SiblingMemberAccessExpression(ISymbol member)
    {
        return SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            member.IsStatic
                ? SyntaxFactory.IdentifierName(member.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
                : SyntaxFactory.ThisExpression(),
            SyntaxFactory.IdentifierName(member.Name));
    }

    public static ArgumentSyntax DefaultArgument(TypeSyntax type)
        => SyntaxFactory.Argument(
            SyntaxFactory.DefaultExpression(type));

    public static LocalDeclarationStatementSyntax LocalVarSingleVariableDeclaration(string variableName, ExpressionSyntax initializer)
        => SyntaxFactory.LocalDeclarationStatement(
            SyntaxFactory.VariableDeclaration(
                SyntaxFactory.IdentifierName("var"),
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.VariableDeclarator(
                        SyntaxFactory.Identifier(variableName),
                        default,
                        SyntaxFactory.EqualsValueClause(initializer)))));

    public static LiteralExpressionSyntax LiteralStringExpression(string str)
        => SyntaxFactory.LiteralExpression(
            SyntaxKind.StringLiteralExpression,
            SyntaxFactory.Literal(str));

    public static LiteralExpressionSyntax LiteralInt32Expression(int value)
        => SyntaxFactory.LiteralExpression(
            SyntaxKind.NumericLiteralExpression,
            SyntaxFactory.Literal(value));

    public static LiteralExpressionSyntax LiteralBooleanExpression(bool flag)
        => SyntaxFactory.LiteralExpression(
            flag ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);

    #endregion
}
