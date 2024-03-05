using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;

namespace Trarizon.TextCommand.SourceGenerator.Utilities.Factories;
internal static class CodeFactory
{
    public static MemberDeclarationSyntax CloneContainingTypeAndNamespaceDeclarations(TypeDeclarationSyntax sourceTypeSyntax, ISymbol sourceMemberSymbol,
        SyntaxList<MemberDeclarationSyntax> members)
    {
        var topType = CloneContainingTypeDeclarations(sourceTypeSyntax, members);

        string nsString = sourceMemberSymbol.ContainingNamespace.ToDisplayString();
        if (nsString == Constants.GlobalNamespace_DisplayString)
            return topType;

        return SyntaxFactory.NamespaceDeclaration(
            SyntaxFactory.ParseName(nsString),
            externs: default,
            usings: default,
            members: SyntaxFactory.SingletonList<MemberDeclarationSyntax>(topType));
    }

    public static TypeDeclarationSyntax CloneContainingTypeDeclarations(TypeDeclarationSyntax sourceSyntax,
        SyntaxList<MemberDeclarationSyntax> members)
    {
        TypeDeclarationSyntax type = ClonePartialDeclaration(sourceSyntax, default, default, members);

        while (sourceSyntax.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault() is { } sourceParent) {
            sourceSyntax = sourceParent;
            type = ClonePartialDeclaration(sourceSyntax, default, default,
                SyntaxFactory.SingletonList<MemberDeclarationSyntax>(type));
        }

        return type;
    }

    public static TypeDeclarationSyntax ClonePartialDeclaration(TypeDeclarationSyntax source,
        SyntaxList<AttributeListSyntax> attributeLists,
        BaseListSyntax? baseList,
        SyntaxList<MemberDeclarationSyntax> members)
        => source switch {
            ClassDeclarationSyntax clz => ClonePartialDeclaration(clz, attributeLists, baseList, members),
            StructDeclarationSyntax str => ClonePartialDeclaration(str, attributeLists, baseList, members),
            RecordDeclarationSyntax rec => ClonePartialDeclaration(rec, attributeLists, baseList, members),
            InterfaceDeclarationSyntax itf => ClonePartialDeclaration(itf, attributeLists, baseList, members),
            _ => throw new InvalidOperationException("Unknown type declaration"),
        };

    /// <summary>
    /// Create a partial class by copy the basic info of original class
    /// </summary>
    public static ClassDeclarationSyntax ClonePartialDeclaration(ClassDeclarationSyntax source,
        SyntaxList<AttributeListSyntax> attributeLists,
        BaseListSyntax? baseList,
        SyntaxList<MemberDeclarationSyntax> members)
        => SyntaxFactory.ClassDeclaration(
            attributeLists,
            SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PartialKeyword)),
            source.Identifier,
            source.TypeParameterList,
            baseList,
            default,
            members);

    /// <summary>
    /// Create a partial struct by copy the basic info of original struct
    /// </summary>
    public static StructDeclarationSyntax ClonePartialDeclaration(StructDeclarationSyntax source,
        SyntaxList<AttributeListSyntax> attributeLists,
        BaseListSyntax? baseList,
        SyntaxList<MemberDeclarationSyntax> members)
        => SyntaxFactory.StructDeclaration(
            attributeLists,
            SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PartialKeyword)),
            source.Identifier,
            source.TypeParameterList,
            baseList,
            default,
            members);

    /// <summary>
    /// Create a partial interface by copy the basic info of original interface
    /// </summary>
    public static InterfaceDeclarationSyntax ClonePartialDeclaration(InterfaceDeclarationSyntax source,
        SyntaxList<AttributeListSyntax> attributeLists,
        BaseListSyntax? baseList,
        SyntaxList<MemberDeclarationSyntax> members)
        => SyntaxFactory.InterfaceDeclaration(
            attributeLists,
            SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PartialKeyword)),
            source.Identifier,
            source.TypeParameterList,
            baseList,
            default,
            members);

    /// <summary>
    /// Create a partial record by copy the basic info of original record
    /// </summary>
    public static RecordDeclarationSyntax ClonePartialDeclaration(RecordDeclarationSyntax source,
        SyntaxList<AttributeListSyntax> attributeLists,
        BaseListSyntax? baseList,
        SyntaxList<MemberDeclarationSyntax> members)
        => SyntaxFactory.RecordDeclaration(
            attributeLists,
            SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PartialKeyword)),
            SyntaxFactory.Token(SyntaxKind.RecordKeyword),
            source.Identifier,
            source.TypeParameterList,
            default,
            baseList,
            default,
            members);

}
