using Microsoft.CodeAnalysis;
using Trarizon.TextCommand.SourceGenerator.Core.Tags;

using MethodParserTuple = (
    Microsoft.CodeAnalysis.IMethodSymbol Symbol,
    Trarizon.TextCommand.SourceGenerator.Core.Tags.MethodParserInputParameterKind InputParameterKind);

namespace Trarizon.TextCommand.SourceGenerator.Core;
internal struct CustomParserInfo
{
    public CustomParserKind Kind { get; private init; }

    private int _enum;
    private ISymbol _memberSymbol;
    private ITypeSymbol _parserReturnType;

    public ImplicitExecutorParameterKind ImplicitKind
    {
        readonly get => (ImplicitExecutorParameterKind)_enum;
        private init => _enum = (int)value;
    }
    public IFieldSymbol Field
    {
        readonly get => (IFieldSymbol)_memberSymbol;
        private init => _memberSymbol = value;
    }
    public IPropertySymbol Property
    {
        readonly get => (IPropertySymbol)_memberSymbol;
        private init => _memberSymbol = value;
    }
    public MethodParserTuple Method
    {
        readonly get => ((IMethodSymbol)_memberSymbol, (MethodParserInputParameterKind)_enum);
        private init => (_memberSymbol, _enum) = (value.Symbol, (int)value.InputParameterKind);
    }
    public ITypeSymbol Struct
    {
        readonly get => (ITypeSymbol)_memberSymbol;
        private init => _memberSymbol = value;
    }

    public ITypeSymbol ParserReturnType
    {
        readonly get => _parserReturnType;
        private init => _parserReturnType = value;
    }

    public static bool TryImplicit(ITypeSymbol type, bool isFlag, out CustomParserInfo parserInfo)
    {
        var iepk = ValidationHelper.ValidateExecutorImplicitParameterKind(type, isFlag, out var parserReturnType);
        if (iepk is ImplicitExecutorParameterKind.Invalid) {
            parserInfo = default;
            return false;
        }
        
        if (!isFlag && iepk is ImplicitExecutorParameterKind.Boolean)
            iepk = ImplicitExecutorParameterKind.ISpanParsable;

        parserInfo = new() {
            Kind = CustomParserKind.Implicit,
            ImplicitKind = iepk,
            ParserReturnType = parserReturnType,
        };
        return true;
    }

    private static bool TryField(IFieldSymbol field, SemanticModel semanticModel, ITypeSymbol targetType, bool isFlag, out CustomParserInfo parserInfo)
    {
        if (ValidationHelper.IsValidParserType(field.Type, semanticModel, targetType, isFlag, out var parserReturnType)) {
            parserInfo = new() {
                Kind = CustomParserKind.Field,
                Field = field,
                ParserReturnType = parserReturnType,
            };
            return true;
        }
        parserInfo = default;
        return false;
    }

    private static bool TryProperty(IPropertySymbol property, SemanticModel semanticModel, ITypeSymbol targetType, bool isFlag, out CustomParserInfo parserInfo)
    {
        if (ValidationHelper.IsValidParserType(property.Type, semanticModel, targetType, isFlag, out var parserReturnType)) {
            parserInfo = new() {
                Kind = CustomParserKind.Property,
                Property = property,
                ParserReturnType = parserReturnType,
            };
            return true;
        }
        parserInfo = default;
        return false;
    }

    private static bool TryMethod(IMethodSymbol method, SemanticModel semanticModel, ITypeSymbol targetType, bool isFlag, out CustomParserInfo parserInfo)
    {
        if (ValidationHelper.IsValidMethodParser(method, semanticModel, targetType, isFlag, out var parserReturnType, out var inputParameterKind)) {
            parserInfo = new() {
                Kind = CustomParserKind.Method,
                Method = (method, inputParameterKind),
                ParserReturnType = parserReturnType,
            };
            return true;
        }
        parserInfo = default;
        return false;
    }

    public static bool TryStruct(ITypeSymbol type, SemanticModel SemanticModel, ITypeSymbol targetType, bool isFlag, out CustomParserInfo parserInfo)
    {
        if (!type.IsValueType) {
            parserInfo = default;
            return false;
        }
        if (ValidationHelper.IsValidParserType(type, SemanticModel, targetType, isFlag, out var parserReturnType)) {
            parserInfo = new() {
                Kind = CustomParserKind.Struct,
                Struct = type,
                ParserReturnType = parserReturnType,
            };
            return true;
        }
        parserInfo = default;
        return false;
    }

    public static bool TryMember(ISymbol member, SemanticModel semanticModel, ITypeSymbol targetType, bool isFlag, out CustomParserInfo parserInfo)
    {
        parserInfo = default;
        return member switch {
            IFieldSymbol field => TryField(field, semanticModel, targetType, isFlag, out parserInfo),
            IPropertySymbol property => TryProperty(property, semanticModel, targetType, isFlag, out parserInfo),
            IMethodSymbol method => TryMethod(method, semanticModel, targetType, isFlag, out parserInfo),
            _ => false,
        };
    }
}
