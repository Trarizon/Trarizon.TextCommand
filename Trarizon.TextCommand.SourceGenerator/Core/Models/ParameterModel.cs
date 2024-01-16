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

    private CLParameterKind _parameterKind;

    public ICLParameterModel CLParameter { get; private set; } = default!;

    private ITypeSymbol? _parsedTypeSymbol;
    public ITypeSymbol ParsedTypeSymbol
    {
        get => _parsedTypeSymbol ??= Symbol.Type;
        private set => _parsedTypeSymbol = value;
    }

    public Filter ValidateSingleAttribute()
    {
        AttributeData? result = null;

        foreach (var attr in Symbol.GetAttributes()) {
            if (result != null) { // multi
                return Filter.Create(DiagnosticFactory.Create(
                    DiagnosticDescriptors.MarkSingleParameterAttributes,
                    Syntax));
            }

            var display = attr.AttributeClass?.ToDisplayString();
            switch (display) {
                case Literals.FlagAttribute_TypeName:
                    _parameterKind = CLParameterKind.Flag;
                    break;
                case Literals.OptionAttribute_TypeName:
                    _parameterKind = CLParameterKind.Option;
                    break;
                case Literals.ValueAttribute_TypeName:
                    _parameterKind = CLParameterKind.Value;
                    break;
                case Literals.MultiValueAttribute_TypeName:
                    _parameterKind = CLParameterKind.MultiValue;
                    break;
                default:
                    continue;
            }
            result = attr;
        }

        _attribute = result;
        return Filter.Success;
    }

    public Filter ValidateCLParameter()
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
                    return Filter.Create(DiagnosticFactory.Create(
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
            return Filter.Create(DiagnosticFactory.Create(err, Syntax));
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

            var parserType = parserMember switch {
                IFieldSymbol field => field.Type,
                IPropertySymbol property => property.Type,
                _ => default,
            };
            if (parserType is null) {
                return DiagnosticDescriptors.CannotFindExplicitParser;
            }

            switch (_parameterKind) {
                case CLParameterKind.Flag: {
                    if (!ValidationHelper.IsCustomParser(parserType, Symbol.Type, out var isFlag) || !isFlag)
                        return DiagnosticDescriptors.CustomFlagParserShouldImplsIArgsFlagParser;
                    return new FlagParameterModel(this) {
                        ParserInfo = (parserType, parserMember),
                        Alias = _attribute.GetConstructorArgument<string>(Literals.FlagAttribute_Alias_CtorParameterIndex),
                        Name = _attribute.GetNamedArgument<string>(Literals.FlagAttribute_Name_PropertyIdentifier)!
                    };
                }
                case CLParameterKind.Option: {
                    if (!ValidationHelper.IsCustomParser(parserType, Symbol.Type, out var isFlag) || isFlag)
                        return DiagnosticDescriptors.CustomParserShouldImplsIArgParser;
                    return new OptionParameterModel(this) {
                        ParserInfo = (parserType, parserMember),
                        Alias = _attribute.GetConstructorArgument<string>(Literals.OptionAttribute_Alias_CtorParameterIndex),
                        Name = _attribute.GetNamedArgument<string>(Literals.OptionAttribute_Name_PropertyIdentifier)!,
                        Required = _attribute.GetNamedArgument<bool>(Literals.OptionAttribute_Required_PropertyIdentifier),
                    };
                }
                case CLParameterKind.Value: {
                    if (!ValidationHelper.IsCustomParser(parserType, Symbol.Type, out var isFlag) || isFlag)
                        return DiagnosticDescriptors.CustomParserShouldImplsIArgParser;
                    return new ValueParameterModel(this) {
                        ParserInfo = (parserType, parserMember),
                        Required = _attribute.GetNamedArgument<bool>(Literals.ValueAttribute_Required_PropertyIdentifier),
                    };
                }
                case CLParameterKind.MultiValue: {
                    var collectionType = ValidationHelper.ValidateMultiValueCollectionType(Symbol.Type, out var elementType, out var elemGetter);
                    if (collectionType == MultiValueCollectionType.Invalid)
                        return DiagnosticDescriptors.InvalidMultiValueCollectionType;
                    if (!ValidationHelper.IsCustomParser(parserType, elementType, out var isFlag))
                        return DiagnosticDescriptors.CustomParserShouldImplsIArgParser;
                    if (isFlag)
                        return DiagnosticDescriptors.MultiValueParserCannotBeFlagParser;

                    ParsedTypeSymbol = elementType;
                    return new MultiValueParameterModel(this) {
                        CollectionType = collectionType,
                        ParserInfo = (parserType, parserMember),
                        ParsedTypeSymbol = elementType,
                        ParsedTypeSyntax = elemGetter(Syntax.Type!),
                        MaxCount = _attribute.GetConstructorArgument<int>(Literals.MultiValueAttribute_MaxCount_CtorParameterIndex),
                        Required = _attribute.GetNamedArgument<bool>(Literals.MultiValueAttribute_Required_PropertyIdentifier),
                    };
                }
                default:
                    throw new InvalidOperationException();
            }

            Result<ICLParameterModel, DiagnosticDescriptor> GetImplicitParserParameterModel()
            {
                switch (_parameterKind) {
                    case CLParameterKind.Flag: {
                        if (ValidationHelper.ValidateImplicitParameterKind(Symbol.Type) == ImplicitCLParameterKind.Boolean)
                            return new FlagParameterModel(this) {
                                ParserInfo = ImplicitCLParameterKind.Boolean,
                                Alias = _attribute.GetConstructorArgument<string>(Literals.FlagAttribute_Alias_CtorParameterIndex),
                                Name = _attribute.GetNamedArgument<string>(Literals.FlagAttribute_Name_PropertyIdentifier)!
                            };
                        else
                            return DiagnosticDescriptors.ParameterNoImplicitParser;
                    }
                    case CLParameterKind.Option: {
                        var implicitParameterKind = ValidationHelper.ValidateImplicitParameterKind(Symbol.Type);
                        // In Non-flag, treat bool as SpanParsable
                        if (implicitParameterKind == ImplicitCLParameterKind.Boolean)
                            implicitParameterKind = ImplicitCLParameterKind.SpanParsable;

                        return new OptionParameterModel(this) {
                            ParserInfo = implicitParameterKind,
                            Alias = _attribute.GetConstructorArgument<string>(Literals.OptionAttribute_Alias_CtorParameterIndex),
                            Name = _attribute.GetNamedArgument<string>(Literals.OptionAttribute_Name_PropertyIdentifier)!,
                            Required = _attribute.GetNamedArgument<bool>(Literals.OptionAttribute_Required_PropertyIdentifier),
                        };
                    }
                    case CLParameterKind.Value: {
                        var implicitParameterKind = ValidationHelper.ValidateImplicitParameterKind(Symbol.Type);
                        // In Non-flag, treat bool as SpanParsable
                        if (implicitParameterKind == ImplicitCLParameterKind.Boolean)
                            implicitParameterKind = ImplicitCLParameterKind.SpanParsable;

                        return new ValueParameterModel(this) {
                            ParserInfo = implicitParameterKind,
                            Required = _attribute.GetNamedArgument<bool>(Literals.ValueAttribute_Required_PropertyIdentifier),
                        };
                    }
                    case CLParameterKind.MultiValue: {
                        var collectionType = ValidationHelper.ValidateMultiValueCollectionType(Symbol.Type, out var elemType, out var elemGetter);
                        if (collectionType == MultiValueCollectionType.Invalid)
                            return DiagnosticDescriptors.ParameterNoImplicitParser;

                        var implicitParameterKind = ValidationHelper.ValidateImplicitParameterKind(elemType);
                        // In Non-flag, treat bool as SpanParsable
                        if (implicitParameterKind == ImplicitCLParameterKind.Boolean)
                            implicitParameterKind = ImplicitCLParameterKind.SpanParsable;

                        ParsedTypeSymbol = elemType;

                        return new MultiValueParameterModel(this) {
                            CollectionType = collectionType,
                            ParserInfo = implicitParameterKind,
                            ParsedTypeSymbol = elemType,
                            ParsedTypeSyntax = elemGetter(Syntax.Type!),
                            MaxCount = _attribute.GetConstructorArgument<int>(Literals.MultiValueAttribute_MaxCount_CtorParameterIndex),
                            Required = _attribute.GetNamedArgument<bool>(Literals.MultiValueAttribute_Required_PropertyIdentifier),
                        };
                    }
                    default:
                        throw new InvalidOperationException();
                }
            }
        }
    }
}
