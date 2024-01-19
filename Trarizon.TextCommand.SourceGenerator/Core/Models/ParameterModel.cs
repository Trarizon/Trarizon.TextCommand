using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models.Parameters;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;
using Trarizon.TextCommand.SourceGenerator.Utilities.Factories;
using Trarizon.TextCommand.SourceGenerator.Utilities.Toolkit;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models;
internal sealed class ParameterModel(ExecutorModel executor, ParameterSyntax syntax, IParameterSymbol symbol)
{
    public ExecutorModel Executor => executor;

    public ParameterSyntax Syntax => syntax;

    public IParameterSymbol Symbol => symbol;

    private AttributeData? _attribute;

    // Values

    public CLParameterKind ParameterKind { get; private set; }

    public ICLParameterModel CLParameter { get; private set; } = default!;

    public Filter ValidateSingleAttribute()
    {
        AttributeData? result = null;

        foreach (var attr in Symbol.GetAttributes()) {
            if (result != null) { // multi
                return Filter.CreateDiagnostic(DiagnosticFactory.Create(
                    DiagnosticDescriptors.MarkSingleParameterAttributes,
                    Syntax));
            }

            var display = attr.AttributeClass?.ToDisplayString();
            switch (display) {
                case Literals.FlagAttribute_TypeName:
                    ParameterKind = CLParameterKind.Flag;
                    break;
                case Literals.OptionAttribute_TypeName:
                    ParameterKind = CLParameterKind.Option;
                    break;
                case Literals.ValueAttribute_TypeName:
                    ParameterKind = CLParameterKind.Value;
                    break;
                case Literals.MultiValueAttribute_TypeName:
                    ParameterKind = CLParameterKind.MultiValue;
                    break;
                default:
                    continue;
            }
            result = attr;
        }

        _attribute = result;
        return Filter.Success;
    }

    public Filter ValidateAndCreateCLParameter()
    {
        if (_attribute is null) { // ParameterKind == Invalid(Implicit)
            var implicitParameterKind = ValidationHelper.ValidateImplicitParameterKind(Symbol.Type);
            switch (implicitParameterKind) {
                case ImplicitCLParameterKind.Boolean:
                    CLParameter = new FlagParameterModel(this) {
                        ParserInfo = implicitParameterKind,
                    };
                    break;
                case ImplicitCLParameterKind.SpanParsable:
                case ImplicitCLParameterKind.Enum:
                case ImplicitCLParameterKind.NullableSpanParsable:
                case ImplicitCLParameterKind.NullableEnum:
                    CLParameter = new OptionParameterModel(this) {
                        ParserInfo = implicitParameterKind,
                    };
                    break;
                default:
                    return Filter.CreateDiagnostic(DiagnosticFactory.Create(
                        DiagnosticDescriptors.ParameterNoImplicitParser,
                        Syntax));
            }
            return Filter.Success;
        }
        else {
            var res = GetCLParameterModel();
            if (res.TryGetValue(out var value, out var err)) {
                CLParameter = value;
                return Filter.Success;
            }
            return Filter.CreateDiagnostic(DiagnosticFactory.Create(err, Syntax));
        }

        Result<ICLParameterModel, DiagnosticDescriptor> GetCLParameterModel()
        {
            var parserAttrArg = _attribute.GetNamedArgument<string>(Literals.ParameterAttribute_ParserPropertyIdentifier);
            if (parserAttrArg is null) {
                return GetImplicitParserParameterModel();
            }

            var parserMember = Executor.Execution.Command.Symbol.GetMembers(parserAttrArg).FirstOrDefault();
            if (parserMember is null) {
                return DiagnosticDescriptors.CannotFindExplicitParser;
            }

            Either<(ITypeSymbol Type, ISymbol Member), IMethodSymbol> parser;
            switch (parserMember) {
                case IFieldSymbol field:
                    parser = (field.Type, parserMember);
                    break;
                case IPropertySymbol property:
                    parser = (property.Type, parserMember);
                    break;
                case IMethodSymbol method:
                    parser = new(method);
                    break;
                default:
                    return DiagnosticDescriptors.CannotFindExplicitParser;
            }

            switch (ParameterKind) {
                case CLParameterKind.Flag: {
                    if (parser.TryGetLeft(out var memberParser, out var methodParser)) {
                        if (!ValidationHelper.IsCustomParser(memberParser.Type, Symbol.Type, out var isFlag) || !isFlag)
                            return DiagnosticDescriptors.CustomFlagParserShouldImplsIArgsFlagParser;
                    }
                    else if (!ValidationHelper.IsValidMethodParser(methodParser, Symbol.Type, CLParameterKind.Flag))
                        return DiagnosticDescriptors.CustomFlagParsingMethodMatchArgFlagParsingDelegate;

                    return new FlagParameterModel(this) {
                        ParserInfo = parser,
                        Alias = _attribute.GetConstructorArgument<string>(Literals.FlagAttribute_Alias_CtorParameterIndex),
                        Name = _attribute.GetNamedArgument<string>(Literals.FlagAttribute_Name_PropertyIdentifier)!
                    };
                }
                case CLParameterKind.Option: {
                    if (parser.TryGetLeft(out var memberParser, out var methodParser)) {
                        if (!ValidationHelper.IsCustomParser(memberParser.Type, Symbol.Type, out var isFlag) || isFlag)
                            return DiagnosticDescriptors.CustomParserShouldImplsIArgParser;
                    }
                    else if (!ValidationHelper.IsValidMethodParser(methodParser, Symbol.Type, ParameterKind))
                        return DiagnosticDescriptors.CustomParsingMethodMatchArgParsingDelegate;

                    return new OptionParameterModel(this) {
                        ParserInfo = parser,
                        Alias = _attribute.GetConstructorArgument<string>(Literals.OptionAttribute_Alias_CtorParameterIndex),
                        Name = _attribute.GetNamedArgument<string>(Literals.OptionAttribute_Name_PropertyIdentifier)!,
                        Required = _attribute.GetNamedArgument<bool>(Literals.OptionAttribute_Required_PropertyIdentifier),
                    };
                }
                case CLParameterKind.Value: {
                    if (parser.TryGetLeft(out var memberParser, out var methodParser)) {
                        if (!ValidationHelper.IsCustomParser(memberParser.Type, Symbol.Type, out var isFlag) || isFlag)
                            return DiagnosticDescriptors.CustomParserShouldImplsIArgParser;
                    }
                    else if (!ValidationHelper.IsValidMethodParser(methodParser, Symbol.Type, ParameterKind))
                        return DiagnosticDescriptors.CustomParsingMethodMatchArgParsingDelegate;

                    return new ValueParameterModel(this) {
                        ParserInfo = parser,
                        Required = _attribute.GetNamedArgument<bool>(Literals.ValueAttribute_Required_PropertyIdentifier),
                    };
                }
                case CLParameterKind.MultiValue: {
                    var collectionType = ValidationHelper.ValidateMultiValueCollectionType(Symbol.Type, out var elementType, out var elemGetter);
                    if (collectionType == MultiValueCollectionType.Invalid)
                        return DiagnosticDescriptors.InvalidMultiValueCollectionType;

                    if (parser.TryGetLeft(out var memberParser, out var methodParser)) {
                        if (!ValidationHelper.IsCustomParser(memberParser.Type, elementType, out var isFlag))
                            return DiagnosticDescriptors.CustomParserShouldImplsIArgParser;
                        if (isFlag)
                            return DiagnosticDescriptors.MultiValueParserCannotBeFlagParser;
                    }
                    else if (!ValidationHelper.IsValidMethodParser(methodParser, Symbol.Type, ParameterKind))
                        return DiagnosticDescriptors.CustomParsingMethodMatchArgParsingDelegate;

                    return new MultiValueParameterModel(this) {
                        CollectionType = collectionType,
                        ParserInfo = parser,
                        ParsedTypeSymbol = elementType,
                        MaxCount = _attribute.GetConstructorArgument<int>(Literals.MultiValueAttribute_MaxCount_CtorParameterIndex),
                        Required = _attribute.GetNamedArgument<bool>(Literals.MultiValueAttribute_Required_PropertyIdentifier),
                    };
                }
                default:
                    throw new InvalidOperationException();
            }

            Result<ICLParameterModel, DiagnosticDescriptor> GetImplicitParserParameterModel()
            {
                switch (ParameterKind) {
                    case CLParameterKind.Flag: {
                        var implicitParameterKind = ValidationHelper.ValidateImplicitParameterKind(Symbol.Type);
                        switch (implicitParameterKind) {
                            case ImplicitCLParameterKind.Boolean:
                                return new FlagParameterModel(this) {
                                    ParserInfo = ImplicitCLParameterKind.Boolean,
                                    Alias = _attribute.GetConstructorArgument<string>(Literals.FlagAttribute_Alias_CtorParameterIndex),
                                    Name = _attribute.GetNamedArgument<string>(Literals.FlagAttribute_Name_PropertyIdentifier)!
                                };
                            case ImplicitCLParameterKind.Invalid:
                            default:
                                return DiagnosticDescriptors.ParameterNoImplicitParser;
                        }
                    }
                    case CLParameterKind.Option: {
                        var implicitParameterKind = ValidationHelper.ValidateImplicitParameterKind(Symbol.Type);
                        switch (implicitParameterKind) {
                            case ImplicitCLParameterKind.Invalid:
                                return DiagnosticDescriptors.ParameterNoImplicitParser;
                            case ImplicitCLParameterKind.Boolean:
                                // In Non-flag, treat bool as SpanParsable
                                implicitParameterKind = ImplicitCLParameterKind.SpanParsable;
                                goto default;
                            default:
                                return new OptionParameterModel(this) {
                                    ParserInfo = implicitParameterKind,
                                    Alias = _attribute.GetConstructorArgument<string>(Literals.OptionAttribute_Alias_CtorParameterIndex),
                                    Name = _attribute.GetNamedArgument<string>(Literals.OptionAttribute_Name_PropertyIdentifier)!,
                                    Required = _attribute.GetNamedArgument<bool>(Literals.OptionAttribute_Required_PropertyIdentifier),
                                };
                        }
                    }
                    case CLParameterKind.Value: {
                        var implicitParameterKind = ValidationHelper.ValidateImplicitParameterKind(Symbol.Type);
                        switch (implicitParameterKind) {
                            case ImplicitCLParameterKind.Invalid:
                                return DiagnosticDescriptors.ParameterNoImplicitParser;
                            case ImplicitCLParameterKind.Boolean:
                                // In Non-flag, treat bool as SpanParsable
                                implicitParameterKind = ImplicitCLParameterKind.SpanParsable;
                                goto default;
                            default:
                                return new ValueParameterModel(this) {
                                    ParserInfo = implicitParameterKind,
                                    Required = _attribute.GetNamedArgument<bool>(Literals.ValueAttribute_Required_PropertyIdentifier),
                                };
                        }
                    }
                    case CLParameterKind.MultiValue: {
                        var collectionType = ValidationHelper.ValidateMultiValueCollectionType(Symbol.Type, out var elemType, out var elemGetter);
                        if (collectionType == MultiValueCollectionType.Invalid)
                            return DiagnosticDescriptors.ParameterNoImplicitParser;

                        var implicitParameterKind = ValidationHelper.ValidateImplicitParameterKind(elemType);
                        switch (implicitParameterKind) {
                            case ImplicitCLParameterKind.Invalid:
                                return DiagnosticDescriptors.ParameterNoImplicitParser;
                            case ImplicitCLParameterKind.Boolean:
                                // In Non-flag, treat bool as SpanParsable
                                implicitParameterKind = ImplicitCLParameterKind.SpanParsable;
                                goto default;
                            default:
                                return new MultiValueParameterModel(this) {
                                    CollectionType = collectionType,
                                    ParserInfo = implicitParameterKind,
                                    ParsedTypeSymbol = elemType,
                                    MaxCount = _attribute.GetConstructorArgument<int>(Literals.MultiValueAttribute_MaxCount_CtorParameterIndex),
                                    Required = _attribute.GetNamedArgument<bool>(Literals.MultiValueAttribute_Required_PropertyIdentifier),
                                };
                        }
                    }
                    default:
                        throw new InvalidOperationException();
                }
            }
        }
    }

    public Filter ValidateRequiredParameterNullableAnnotation()
    {
        if (CLParameter is IRequiredParameterModel requiredParameter &&
            !requiredParameter.Required &&
            !ValidationHelper.IsCanBeDefault(Symbol.Type)
        ) {
            return Filter.CreateDiagnostic(DiagnosticFactory.Create(
                DiagnosticDescriptors.NotRequiredParameterMayBeDefault,
                Syntax.Type!));
        }

        return Filter.Success;
    }
}
