using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics;
using System.Linq;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models.Parameters;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;
using Trarizon.TextCommand.SourceGenerator.Utilities.Factories;
using Trarizon.TextCommand.SourceGenerator.Utilities.Toolkit;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models;
internal sealed class ParameterModel(ExecutorModel executor, ParameterSyntax syntax, IParameterSymbol symbol)
{
    public ExecutorModel Executor { get; } = executor;

    public ParameterSyntax Syntax { get; } = syntax;

    public IParameterSymbol Symbol { get; } = symbol;

    private AttributeData? _attribute;

    // Values

    public ParameterKind ParameterKind { get; private set; }

    private IParameterData? _parameterData;
    public IParameterData ParameterData
    {
        get => _parameterData ?? throw new InvalidOperationException("Parameter not set");
        set => _parameterData = value;
    }

    public Filter ValidateSingleAttribute_SetParameterKind()
    {
        if (Symbol.GetAttributes().TrySingleOrNone(attr =>
        {
            var display = attr!.AttributeClass?.ToDisplayString();
            switch (display) {
                case Literals.FlagAttribute_TypeName:
                    ParameterKind = ParameterKind.Flag;
                    break;
                case Literals.OptionAttribute_TypeName:
                    ParameterKind = ParameterKind.Option;
                    break;
                case Literals.ValueAttribute_TypeName:
                    ParameterKind = ParameterKind.Value;
                    break;
                case Literals.MultiValueAttribute_TypeName:
                    ParameterKind = ParameterKind.MultiValue;
                    break;
                default:
                    return false;
            }
            return true;
        }, out _attribute)) {
            return Filter.Success;
        }

        return Filter.CreateDiagnostic(DiagnosticFactory.Create(
            DiagnosticDescriptors.MarkSingleParameterAttributes,
            Syntax));
    }

    public Filter ValidateParameterData_SetParameterData()
    {
        Debug.Assert(_attribute is null || ParameterKind != ParameterKind.Invalid);

        Result<(IParameterData Parameter, Diagnostic? NullableWarning), Diagnostic> res = _attribute is null
            ? GetImplicitParserParameterData().Select(data => (data, default(Diagnostic)))
            : GetParameterData();

        if (res.TryGetValue(out var val, out var err)) {
            ParameterData = val.Parameter;
            if (val.NullableWarning is not null)
                return Filter.CreateDiagnostic(val.NullableWarning);
            else
                return Filter.Success;
        }
        else {
            return Filter.CreateDiagnostic(err);
        }

        Result<(IParameterData Parameter, Diagnostic? NullableWarning), Diagnostic> GetParameterData()
        {
            var parserAttrArg = _attribute.GetNamedArgument<string>(Literals.ParameterAttribute_ParserPropertyIdentifier);
            if (parserAttrArg is null) {
                return GetImplicitParserParameterData().Select(data => (data, default(Diagnostic)));
            }

            return GetMemberParserInfo(parserAttrArg)
                 .SelectWrapped(GetExplicitParserParameterData);
        }

        Result<ParserInfoProvider, Diagnostic> GetMemberParserInfo(string customParserName)
        {
            var parserMember = Executor.Execution.Command.Symbol.GetMembers(customParserName).FirstOrDefault();
            return parserMember switch {
                IFieldSymbol field => new ParserInfoProvider(field.Type, field),
                IPropertySymbol property => new ParserInfoProvider(property.Type, property),
                IMethodSymbol method => new ParserInfoProvider(method),
                _ => DiagnosticFactory.Create(
                    DiagnosticDescriptors.CannotFindExplicitParser_1,
                    Syntax,
                    customParserName),
            };
        }

        Result<(IParameterData Parameter, Diagnostic? NullableWarning), Diagnostic> GetExplicitParserParameterData(ParserInfoProvider parserInfo)
        {
            bool nullableNotMatched;
            IParameterData parameter;

            switch (ParameterKind) {
                case ParameterKind.Flag: {
                    if (ValidateParser(Symbol.Type, true, out var parsedType, out nullableNotMatched) is { } err)
                        return err;
                    parameter = new FlagParameterData(this) {
                        ParserInfo = parserInfo,
                        ParsedTypeSymbol = parsedType,
                        Alias = _attribute.GetConstructorArgument<string>(Literals.FlagAttribute_Alias_CtorParameterIndex),
                        Name = _attribute.GetNamedArgument<string>(Literals.FlagAttribute_Name_PropertyIdentifier)!
                    };
                    break;
                }

                case ParameterKind.Option: {
                    if (ValidateParser(Symbol.Type, false, out var parsedType, out nullableNotMatched) is { } err)
                        return err;
                    parameter = new OptionParameterData(this) {
                        ParserInfo = parserInfo,
                        ParsedTypeSymbol = parsedType,
                        Alias = _attribute.GetConstructorArgument<string>(Literals.OptionAttribute_Alias_CtorParameterIndex),
                        Name = _attribute.GetNamedArgument<string>(Literals.OptionAttribute_Name_PropertyIdentifier)!,
                        Required = _attribute.GetNamedArgument<bool>(Literals.OptionAttribute_Required_PropertyIdentifier),
                    };
                    break;
                }

                case ParameterKind.Value: {
                    if (ValidateParser(Symbol.Type, false, out var parsedType, out nullableNotMatched) is { } err)
                        return err;
                    parameter = new ValueParameterData(this) {
                        ParserInfo = parserInfo,
                        ParsedTypeSymbol = parsedType,
                        Required = _attribute.GetNamedArgument<bool>(Literals.ValueAttribute_Required_PropertyIdentifier),
                    };
                    break;
                }

                case ParameterKind.MultiValue: {
                    var collectionType = ValidationHelper.ValidateMultiValueCollectionType(Symbol.Type, out var elementType);
                    if (collectionType is MultiValueCollectionType.Invalid) {
                        return DiagnosticFactory.Create(
                            DiagnosticDescriptors.InvalidMultiValueCollectionType,
                            Syntax);
                    }

                    if (ValidateParser(elementType, false, out var parsedType, out nullableNotMatched) is { } err)
                        return err;

                    parameter = new MultiValueParameterData(this) {
                        CollectionType = collectionType,
                        ParserInfo = parserInfo,
                        ParsedTypeSymbol = parsedType,
                        MaxCount = _attribute.GetConstructorArgument<int>(Literals.MultiValueAttribute_MaxCount_CtorParameterIndex),
                        Required = _attribute.GetNamedArgument<bool>(Literals.MultiValueAttribute_Required_PropertyIdentifier),
                    };
                    break;
                }

                default:
                    throw new InvalidOperationException();
            }

            if (nullableNotMatched) {
                return (parameter, DiagnosticFactory.Create(
                    DiagnosticDescriptors.ParsedArgumentMaybeNull,
                    Syntax));
            }
            else {
                return (parameter, null);
            }

            Diagnostic? ValidateParser(ITypeSymbol assignedType, bool isFlag, out ITypeSymbol parsedType, out bool nullableNotMatched)
            {
                SemanticModel semanticModel = Executor.Execution.Command.SemanticModel;

                switch (parserInfo.Kind) {
                    case ParserKind.FieldOrProperty:
                        if (!ValidationHelper.IsCustomParser(semanticModel, parserInfo.FieldOrProperty.Type, assignedType, isFlag, out parsedType!, out nullableNotMatched)) {
                            return DiagnosticFactory.Create(isFlag
                                ? DiagnosticDescriptors.CustomFlagParserShouldImplsIArgsFlagParser
                                : DiagnosticDescriptors.CustomParserShouldImplsIArgParser,
                                Syntax);
                        }
                        break;
                    case ParserKind.Method:
                        if (!ValidationHelper.IsValidMethodParser(semanticModel, parserInfo.Method, assignedType, isFlag, out parsedType!, out nullableNotMatched)) {
                            return DiagnosticFactory.Create(isFlag
                                ? DiagnosticDescriptors.CustomFlagParsingMethodMatchArgFlagParsingDelegate
                                : DiagnosticDescriptors.CustomParsingMethodMatchArgParsingDelegate,
                                Syntax);
                        }
                        break;
                    default: // Invalid or Implicit
                        throw new InvalidOperationException();
                }
                return null;
            }
        }

        Result<IParameterData, Diagnostic> GetImplicitParserParameterData()
        {
            switch (ParameterKind) {
                // Implicit parameter
                case ParameterKind.Invalid: {
                    var implicitParameterKind = ValidationHelper.ValidateImplicitParameterKind(Symbol.Type);

                    return implicitParameterKind switch {
                        ImplicitParameterKind.Boolean => new FlagParameterData(this) {
                            ParserInfo = new ParserInfoProvider(implicitParameterKind),
                        },
                        ImplicitParameterKind.SpanParsable or
                        ImplicitParameterKind.Enum or
                        ImplicitParameterKind.NullableSpanParsable or
                        ImplicitParameterKind.NullableEnum => new OptionParameterData(this) {
                            ParserInfo = new ParserInfoProvider(implicitParameterKind),
                        },
                        _ => DiagnosticFactory.Create(
                            DiagnosticDescriptors.ParameterNoImplicitParser,
                            Syntax),
                    };
                }

                // In non-invalid case, _attribute is not null
                // Flag
                case ParameterKind.Flag: {
                    var implicitParameterKind = ValidationHelper.ValidateImplicitParameterKind(Symbol.Type);
                    return implicitParameterKind switch {
                        ImplicitParameterKind.Boolean => new FlagParameterData(this) {
                            ParserInfo = new ParserInfoProvider(implicitParameterKind),
                            Alias = _attribute!.GetConstructorArgument<string>(Literals.FlagAttribute_Alias_CtorParameterIndex),
                            Name = _attribute!.GetNamedArgument<string>(Literals.FlagAttribute_Name_PropertyIdentifier)!
                        },
                        _ => DiagnosticFactory.Create(
                            DiagnosticDescriptors.ParameterNoImplicitParser,
                            Syntax),
                    };
                }

                // Option
                case ParameterKind.Option: {
                    var implicitParameterKind = ValidationHelper.ValidateImplicitParameterKind(Symbol.Type);
                    if (implicitParameterKind is ImplicitParameterKind.Boolean)
                        implicitParameterKind = ImplicitParameterKind.SpanParsable;

                    return implicitParameterKind switch {
                        ImplicitParameterKind.Invalid => DiagnosticFactory.Create(
                            DiagnosticDescriptors.ParameterNoImplicitParser,
                            Syntax),
                        _ => new OptionParameterData(this) {
                            ParserInfo = new ParserInfoProvider(implicitParameterKind),
                            Alias = _attribute!.GetConstructorArgument<string>(Literals.OptionAttribute_Alias_CtorParameterIndex),
                            Name = _attribute!.GetNamedArgument<string>(Literals.OptionAttribute_Name_PropertyIdentifier)!,
                            Required = _attribute!.GetNamedArgument<bool>(Literals.OptionAttribute_Required_PropertyIdentifier),
                        },
                    };
                }

                // Value
                case ParameterKind.Value: {
                    var implicitParameterKind = ValidationHelper.ValidateImplicitParameterKind(Symbol.Type);
                    if (implicitParameterKind is ImplicitParameterKind.Boolean)
                        implicitParameterKind = ImplicitParameterKind.SpanParsable;

                    return implicitParameterKind switch {
                        ImplicitParameterKind.Invalid => DiagnosticFactory.Create(
                            DiagnosticDescriptors.ParameterNoImplicitParser,
                            Syntax),
                        _ => new ValueParameterData(this) {
                            ParserInfo = new ParserInfoProvider(implicitParameterKind),
                            Required = _attribute!.GetNamedArgument<bool>(Literals.ValueAttribute_Required_PropertyIdentifier),
                        },
                    };
                }

                // MultiValue
                case ParameterKind.MultiValue: {
                    var collectionType = ValidationHelper.ValidateMultiValueCollectionType(Symbol.Type, out var elemType);
                    if (collectionType == MultiValueCollectionType.Invalid)
                        return DiagnosticFactory.Create(
                            DiagnosticDescriptors.ParameterNoImplicitParser,
                            Syntax);

                    var implicitParameterKind = ValidationHelper.ValidateImplicitParameterKind(elemType);
                    if (implicitParameterKind is ImplicitParameterKind.Boolean)
                        implicitParameterKind = ImplicitParameterKind.SpanParsable;

                    return implicitParameterKind switch {
                        ImplicitParameterKind.Invalid => DiagnosticFactory.Create(
                            DiagnosticDescriptors.ParameterNoImplicitParser,
                            Syntax),
                        _ => new MultiValueParameterData(this) {
                            CollectionType = collectionType,
                            ParserInfo = new ParserInfoProvider(implicitParameterKind),
                            ParsedTypeSymbol = elemType,
                            MaxCount = _attribute!.GetConstructorArgument<int>(Literals.MultiValueAttribute_MaxCount_CtorParameterIndex),
                            Required = _attribute!.GetNamedArgument<bool>(Literals.MultiValueAttribute_Required_PropertyIdentifier),
                        },
                    };
                }

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    public Filter ValidateRequiredParameterNullableAnnotation()
    {
        if (ParameterData is IRequiredParameterData requiredParameter and not MultiValueParameterData &&
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
