using Microsoft.CodeAnalysis;

namespace Trarizon.TextCommand.SourceGenerator.Core;
internal readonly struct ParserInfoProvider
{
    public ParserKind Kind { get; }

    private readonly ImplicitParameterKind _implicitCLParameterKind;
    public ImplicitParameterKind ImplicitCLParameterKind => _implicitCLParameterKind;

    private readonly ITypeSymbol? _memberType;
    private readonly ISymbol? _member;
    public (ITypeSymbol Type, ISymbol Member) FieldOrProperty => (_memberType!, _member!);
    public IMethodSymbol Method => (IMethodSymbol)_member!;

    public ParserInfoProvider(ImplicitParameterKind implicitCLParameterKind)
    {
        Kind = ParserKind.Implicit;
        _implicitCLParameterKind = implicitCLParameterKind;
    }

    public ParserInfoProvider(ITypeSymbol fieldOrPropertyType, ISymbol fieldOrPropertyMember)
    {
        Kind = ParserKind.FieldOrProperty;
        (_memberType, _member) = (fieldOrPropertyType, fieldOrPropertyMember);
    }

    public ParserInfoProvider(IMethodSymbol method)
    {
        Kind = ParserKind.Method;
        _member = method;
    }
}
