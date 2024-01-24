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

    public Filter ValidateSingleAttribute_SetParameterKind()
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

    public Filter ValidateCLParameter_SetCLParameter()
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
            if (!res.IsClosed)
                CLParameter = res.Context;
            return res;
        }

        Filter<ICLParameterModel> GetCLParameterModel()
        {
            var parserAttrArg = _attribute.GetNamedArgument<string>(Literals.ParameterAttribute_ParserPropertyIdentifier);
            if (parserAttrArg is null) {
                if (GetImplicitParserParameterModel().TryGetValue(out var val, out var err))
                    return Filter.Create(val);
                else
                    return Filter.CreateDiagnostic<ICLParameterModel>(DiagnosticFactory.Create(err, Syntax));
            }

            if (!GetMemberParser(parserAttrArg).TryGetValue(out var parser, out var err2)) {
                return Filter.CreateDiagnostic<ICLParameterModel>(DiagnosticFactory.Create(err2, Syntax));
            }

            var semanticModel = Executor.Execution.Context.SemanticModel;
            bool nullableNotMatched;
            ITypeSymbol? parsedType;
            ICLParameterModel result;
            switch (ParameterKind) {
                case CLParameterKind.Flag: {
                    if (parser.TryGetLeft(out var memberParser, out var methodParser)) {
                        if (!ValidationHelper.IsCustomParser(semanticModel, memberParser.Type, Symbol.Type, out parsedType, out var isFlag, out nullableNotMatched) || !isFlag) {
                            return Filter.CreateDiagnostic<ICLParameterModel>(DiagnosticFactory.Create(
                                DiagnosticDescriptors.CustomFlagParserShouldImplsIArgsFlagParser, Syntax));
                        }
                    }
                    else if (!ValidationHelper.IsValidMethodParser(semanticModel, methodParser, Symbol.Type, CLParameterKind.Flag, out parsedType, out nullableNotMatched)) {
                        return Filter.CreateDiagnostic<ICLParameterModel>(DiagnosticFactory.Create(
                            DiagnosticDescriptors.CustomFlagParsingMethodMatchArgFlagParsingDelegate, Syntax));
                    }

                    result = new FlagParameterModel(this) {
                        ParserInfo = parser,
                        ParsedTypeSymbol = parsedType,
                        Alias = _attribute.GetConstructorArgument<string>(Literals.FlagAttribute_Alias_CtorParameterIndex),
                        Name = _attribute.GetNamedArgument<string>(Literals.FlagAttribute_Name_PropertyIdentifier)!
                    };
                    break;
                }
                case CLParameterKind.Option: {
                    if (parser.TryGetLeft(out var memberParser, out var methodParser)) {
                        if (!ValidationHelper.IsCustomParser(semanticModel, memberParser.Type, Symbol.Type, out parsedType, out var isFlag, out nullableNotMatched) || isFlag) {
                            return Filter.CreateDiagnostic<ICLParameterModel>(DiagnosticFactory.Create(
                                DiagnosticDescriptors.CustomParserShouldImplsIArgParser, Syntax));
                        }
                    }
                    else if (!ValidationHelper.IsValidMethodParser(semanticModel, methodParser, Symbol.Type, ParameterKind, out parsedType, out nullableNotMatched)) {
                        return Filter.CreateDiagnostic<ICLParameterModel>(DiagnosticFactory.Create(
                            DiagnosticDescriptors.CustomParsingMethodMatchArgParsingDelegate, Syntax));
                    }

                    result = new OptionParameterModel(this) {
                        ParserInfo = parser,
                        ParsedTypeSymbol = parsedType,
                        Alias = _attribute.GetConstructorArgument<string>(Literals.OptionAttribute_Alias_CtorParameterIndex),
                        Name = _attribute.GetNamedArgument<string>(Literals.OptionAttribute_Name_PropertyIdentifier)!,
                        Required = _attribute.GetNamedArgument<bool>(Literals.OptionAttribute_Required_PropertyIdentifier),
                    };
                    break;
                }
                case CLParameterKind.Value: {
                    if (parser.TryGetLeft(out var memberParser, out var methodParser)) {
                        if (!ValidationHelper.IsCustomParser(semanticModel, memberParser.Type, Symbol.Type, out parsedType, out var isFlag, out nullableNotMatched) || isFlag)
                            return Filter.CreateDiagnostic<ICLParameterModel>(DiagnosticFactory.Create(
                                DiagnosticDescriptors.CustomParserShouldImplsIArgParser, Syntax));
                    }
                    else if (!ValidationHelper.IsValidMethodParser(semanticModel, methodParser, Symbol.Type, ParameterKind, out parsedType, out nullableNotMatched)) {
                        return Filter.CreateDiagnostic<ICLParameterModel>(DiagnosticFactory.Create(
                            DiagnosticDescriptors.CustomParsingMethodMatchArgParsingDelegate, Syntax));
                    }

                    result = new ValueParameterModel(this) {
                        ParserInfo = parser,
                        ParsedTypeSymbol = parsedType,
                        Required = _attribute.GetNamedArgument<bool>(Literals.ValueAttribute_Required_PropertyIdentifier),
                    };
                    break;
                }
                case CLParameterKind.MultiValue: {
                    var collectionType = ValidationHelper.ValidateMultiValueCollectionType(Symbol.Type, out var elementType, out var elemGetter);
                    if (collectionType == MultiValueCollectionType.Invalid) {
                        return Filter.CreateDiagnostic<ICLParameterModel>(DiagnosticFactory.Create(
                            DiagnosticDescriptors.InvalidMultiValueCollectionType, Syntax));
                    }

                    if (parser.TryGetLeft(out var memberParser, out var methodParser)) {
                        if (!ValidationHelper.IsCustomParser(semanticModel, memberParser.Type, elementType, out parsedType, out var isFlag, out nullableNotMatched)) {
                            return Filter.CreateDiagnostic<ICLParameterModel>(DiagnosticFactory.Create(
                                DiagnosticDescriptors.CustomParserShouldImplsIArgParser, Syntax));
                        }
                        if (isFlag) {
                            return Filter.CreateDiagnostic<ICLParameterModel>(DiagnosticFactory.Create(
                                DiagnosticDescriptors.MultiValueParserCannotBeFlagParser, Syntax));
                        }
                    }
                    else if (!ValidationHelper.IsValidMethodParser(semanticModel, methodParser, elementType, ParameterKind, out parsedType, out nullableNotMatched)) {
                        return Filter.CreateDiagnostic<ICLParameterModel>(DiagnosticFactory.Create(
                            DiagnosticDescriptors.CustomParsingMethodMatchArgParsingDelegate, Syntax));
                    }

                    result = new MultiValueParameterModel(this) {
                        CollectionType = collectionType,
                        ParserInfo = parser,
                        ParsedTypeSymbol = elementType,
                        MaxCount = _attribute.GetConstructorArgument<int>(Literals.MultiValueAttribute_MaxCount_CtorParameterIndex),
                        Required = _attribute.GetNamedArgument<bool>(Literals.MultiValueAttribute_Required_PropertyIdentifier),
                    };
                    break;
                }
                default:
                    throw new InvalidOperationException();
            }

            if (nullableNotMatched) {
                return Filter.Create(result).Predicate(_ => Filter.CreateDiagnostic(DiagnosticFactory.Create(
                    DiagnosticDescriptors.ParsedArgumentMaybeNull, Syntax)));
            }
            else {
                return Filter.Create(result);
            }

            #region Util Methods

            Result<Either<(ITypeSymbol Type, ISymbol Member), IMethodSymbol>, DiagnosticDescriptor> GetMemberParser(string customParserName)
            {
                var parserMember = Executor.Execution.Command.Symbol.GetMembers(customParserName).FirstOrDefault();
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

                return parser;
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

            #endregion
        }
    }

    public Filter ValidateRequiredParameterNullableAnnotation()
    {
        if (CLParameter is IRequiredParameterModel requiredParameter and not MultiValueParameterModel &&
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
