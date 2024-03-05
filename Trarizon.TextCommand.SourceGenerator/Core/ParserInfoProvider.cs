using Microsoft.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Trarizon.TextCommand.SourceGenerator.Core;
internal readonly struct ParserInfoProvider
{
    public static readonly ParserInfoProvider Invalid = default;

    public ParserKind Kind { get; }

    public ImplicitParameterKind ImplicitParameterKind { get; }

    private readonly (ITypeSymbol, ISymbol) _member;
    public (ITypeSymbol Type, ISymbol Member) FieldOrProperty => _member;
    public IMethodSymbol Method => (IMethodSymbol)_member.Item2;
    public ITypeSymbol Struct => (ITypeSymbol)_member.Item2;

    public ParserInfoProvider(ImplicitParameterKind implicitParameterKind)
    {
        Kind = ParserKind.Implicit;
        ImplicitParameterKind = implicitParameterKind;
    }

    public ParserInfoProvider(ITypeSymbol fieldOrPropertyType, ISymbol fieldOrPropertyMember)
    {
        Kind = ParserKind.FieldOrProperty;
        _member = (fieldOrPropertyType, fieldOrPropertyMember);
    }

    public ParserInfoProvider(IMethodSymbol method)
    {
        Kind = ParserKind.Method;
        _member.Item2 = method;
    }

    public ParserInfoProvider(ITypeSymbol structType)
    {
        Kind = ParserKind.Struct;
        _member.Item2 = structType;
    }

    public enum ParserKind
    {
        Invalid = 0,
        Implicit,
        FieldOrProperty,
        Method,
        Struct,
    }
}
