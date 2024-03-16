using Microsoft.CodeAnalysis;
using Trarizon.TextCommand.SourceGenerator.Core.Tags;

namespace Trarizon.TextCommand.SourceGenerator.Core;
internal struct ParserInfoProvider
{
    public static readonly ParserInfoProvider Invalid = default;

    public ParserKind Kind { get; }

    private ITypeSymbol? _memberTypeSymbol;
    private ISymbol? _memberSymbol;
    private int _enum;

    public readonly ImplicitParameterKind ImplicitParameterKind
    {
        get => (ImplicitParameterKind)_enum;
        private init => _enum = (int)value;
    }

    /// <summary>
    /// Type of field or property
    /// </summary>
    public ITypeSymbol MemberTypeSymbol
    {
        readonly get => _memberTypeSymbol!;
        private init => _memberTypeSymbol = value;
    }

    /// <summary>
    /// Symbol of field or property (or method)
    /// </summary>
    public ISymbol MemberSymbol
    {
        readonly get => _memberSymbol!;
        private init => _memberSymbol = value;
    }

    public IMethodSymbol MethodMemberSymbol
    {
        readonly get => (IMethodSymbol)_memberSymbol!;
        private init => _memberSymbol = value;
    }

    public MethodParserInputKind MethodParserInputKind
    {
        readonly get => (MethodParserInputKind)_enum;
        set => _enum = (int)value;
    }

    public ITypeSymbol StructSymbol
    {
        readonly get => _memberTypeSymbol!;
        private init => _memberTypeSymbol = value;
    }

    public ParserInfoProvider(ImplicitParameterKind implicitParameterKind)
    {
        Kind = ParserKind.Implicit;
        ImplicitParameterKind = implicitParameterKind;
    }

    public ParserInfoProvider(ITypeSymbol fieldOrPropertyType, ISymbol fieldOrPropertyMember)
    {
        Kind = ParserKind.FieldOrProperty;
        MemberSymbol = fieldOrPropertyMember;
        MemberTypeSymbol = fieldOrPropertyType;
    }

    public ParserInfoProvider(IMethodSymbol method)
    {
        Kind = ParserKind.Method;
        MethodMemberSymbol = method;
    }

    public ParserInfoProvider(ITypeSymbol structType)
    {
        Kind = ParserKind.Struct;
        StructSymbol = structType;
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
