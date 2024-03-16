using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
using Trarizon.TextCommand.SourceGenerator.Core.Tags;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;
using Trarizon.TextCommand.SourceGenerator.Utilities.Factories;
using Trarizon.TextCommand.SourceGenerator.Utilities.Toolkit;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models;
internal sealed class ParameterModel(ExecutorModel executor)
{
    [MemberNotNullWhen(true, nameof(ParameterData))]
    public bool IsValid => _parameterData is not null;

    public SemanticModel SemanticModel => Executor.SemanticModel;

    public ExecutorModel Executor { get; } = executor;

    public required ParameterSyntax Syntax { get; init; }
    public required IParameterSymbol Symbol { get; init; }

    private AttributeData? _attribute;

    // Data

    public ParameterKind ParameterKind { get; private set; }

    private IParameterData? _parameterData;
    /// <summary>
    /// Not null is <see cref="IsValid"/><br/>
    /// Value set in <see cref="ValidateParameterData"/>
    /// </summary>
    public IParameterData? ParameterData
    {
        get => _parameterData;
        set => _parameterData = value;
    }

    // Validation

    public Diagnostic? ValidateSingleAttribute()
    {
        bool isSingleAttr = Symbol.GetAttributes()
            .Select(attr => (attr, EnumHelper.GetParameterKind(attr)))
            .TrySingleOrNone(attr => attr.Item2 != ParameterKind.Invalid, out var res);

        if (isSingleAttr) {
            (_attribute, ParameterKind) = res;
            return null;
        }

        return DiagnosticFactory.Create(
            DiagnosticDescriptors.MarkSingleParameterAttributes,
            Syntax);
    }

    public Diagnostic? ValidateParameterData()
    {
        // Debug.Assert(_attribute is null || ParameterKind != ParameterKind.Invalid);

        var (parameter, diagnostic) = _attribute is null
            ? GetImplicitParserParameterData().ToTuple()
            : GetParameterData();

        if (parameter is not null) {
            ParameterData = parameter;
        }
        return diagnostic;

        (IParameterData? Parameter, Diagnostic? Diagnostic) GetParameterData()
        {
            ParserInfoProvider memberParserInfo;

            var memberParserAttrArg = _attribute.GetNamedArgument<string>(Literals.ParameterAttribute_Parser_PropertyIdentifier);
            var typeParserAttrArg = _attribute.GetNamedArgument<ITypeSymbol>(Literals.ParameterAttribute_ParserType_PropertyIdentifier);

            switch (memberParserAttrArg, typeParserAttrArg) {
                case (null, null):
                    // use implicit parser
                    GetImplicitParserParameterData().TryGetValue(out var val, out var err);
                    return (val, err);

                case (not null, not null):
                    return (null, DiagnosticFactory.Create(
                        DiagnosticDescriptors.DoNotSpecifyBothParserAndParserType,
                        Syntax));

                case (not null, null):
                    var parserMember = Executor.Execution.Command.Symbol.GetMembers(memberParserAttrArg).FirstOrDefault();
                    memberParserInfo = parserMember switch {
                        IFieldSymbol field => new ParserInfoProvider(field.Type, field),
                        IPropertySymbol property => new ParserInfoProvider(property.Type, property),
                        IMethodSymbol method => new ParserInfoProvider(method),
                        _ => ParserInfoProvider.Invalid,
                    };
                    break;

                case (null, not null):
                    memberParserInfo = new ParserInfoProvider(typeParserAttrArg);
                    break;
            }

            if (memberParserInfo.Kind is ParserInfoProvider.ParserKind.Invalid)
                return (null, DiagnosticFactory.Create(
                    DiagnosticDescriptors.CannotFindExplicitParser_0MemberName,
                    Syntax,
                    memberParserAttrArg));

            return GetExplicitParserParameterData(memberParserInfo);
        }

        (IParameterData? Parameter, Diagnostic? Diagnostic) GetExplicitParserParameterData(ParserInfoProvider parserInfo)
        {
            switch (ParameterKind) {
                case ParameterKind.Flag: {
                    if (ValidateParser(ref parserInfo, Symbol.Type, true, out var parsedType, out bool nullableNotMatched) is { } err) {
                        return (null, err);
                    }
                    return (new FlagParameterData(this) {
                        ParserInfo = parserInfo,
                        ResultTypeSymbol = parsedType,
                        Alias = _attribute.GetConstructorArgument<string>(Literals.FlagAttribute_Alias_CtorParameterIndex),
#pragma warning disable CS8601
                        Name = _attribute.GetNamedArgument<string>(Literals.FlagAttribute_Name_PropertyIdentifier),
#pragma warning restore CS8601
                    }, nullableNotMatched ? NullableNotMatchedError() : null);
                }

                case ParameterKind.Option: {
                    if (ValidateParser(ref parserInfo, Symbol.Type, false, out var parsedType, out var nullableNotMatched) is { } err) {
                        return (null, err);
                    }
                    return (new OptionParameterData(this) {
                        ParserInfo = parserInfo,
                        ParsedTypeSymbol = parsedType,
                        Alias = _attribute.GetConstructorArgument<string>(Literals.OptionAttribute_Alias_CtorParameterIndex),
#pragma warning disable CS8601
                        Name = _attribute.GetNamedArgument<string>(Literals.OptionAttribute_Name_PropertyIdentifier),
#pragma warning restore CS8601
                        Required = _attribute.GetNamedArgument<bool>(Literals.OptionAttribute_Required_PropertyIdentifier),
                    }, nullableNotMatched ? NullableNotMatchedError() : null);
                }

                case ParameterKind.Value: {
                    if (ValidateParser(ref parserInfo, Symbol.Type, false, out var parsedType, out var nullableNotMatched) is { } err) {
                        return (null, err);
                    }
                    return (new ValueParameterData(this) {
                        ParserInfo = parserInfo,
                        ParsedTypeSymbol = parsedType,
                        Required = _attribute.GetNamedArgument<bool>(Literals.ValueAttribute_Required_PropertyIdentifier),
                    }, nullableNotMatched ? NullableNotMatchedError() : null);
                }

                case ParameterKind.MultiValue: {
                    var collectionType = ValidationHelper.ValidateMultiCollectionType(Symbol.Type, out var elementType);
                    if (collectionType is MultiValueCollectionType.Invalid) {
                        return (null, DiagnosticFactory.Create(
                            DiagnosticDescriptors.InvalidMultiValueCollectionType,
                            Syntax));
                    }

                    if (ValidateParser(ref parserInfo, elementType, false, out var parsedType, out var nullableNotMatched) is { } err) {
                        return (null, err);
                    }

                    return (new MultiValueParameterData(this) {
                        CollectionType = collectionType,
                        ParserInfo = parserInfo,
                        ResultTypeSymbol = elementType,
                        ParsedTypeSymbol = parsedType,
                        MaxCount = _attribute.GetConstructorArgument<int>(Literals.MultiValueAttribute_MaxCount_CtorParameterIndex),
                        Required = _attribute.GetNamedArgument<bool>(Literals.ValueAttribute_Required_PropertyIdentifier),
                    }, nullableNotMatched ? NullableNotMatchedError() : null);
                }

                default:
                    throw new InvalidOperationException();
            }

            Diagnostic NullableNotMatchedError()
                => DiagnosticFactory.Create(
                    DiagnosticDescriptors.ParsedArgumentMaybeNull,
                    Syntax);

        }

        Diagnostic? ValidateParser(ref ParserInfoProvider parserInfo, ITypeSymbol assignedType, bool isFlag, /* NullIfReturnNotNull */ out ITypeSymbol parsedType, out bool nullableNotMatched)
        {
            switch (parserInfo.Kind) {
                case ParserInfoProvider.ParserKind.FieldOrProperty:
                    if (!ValidationHelper.IsValidParserType(SemanticModel, parserInfo.MemberTypeSymbol, assignedType, isFlag, out parsedType!, out nullableNotMatched)) {
                        return DiagnosticFactory.Create(isFlag
                            ? DiagnosticDescriptors.CustomFlagParserShouldImplsIArgsFlagParser
                            : DiagnosticDescriptors.CustomParserShouldImplsIArgParser,
                            Syntax);
                    }
                    break;
                case ParserInfoProvider.ParserKind.Method:
                    if (!ValidationHelper.IsValidMethodParser(SemanticModel, parserInfo.MethodMemberSymbol, assignedType, isFlag, out parsedType!, out var inputKind, out nullableNotMatched)) {
                        return DiagnosticFactory.Create(isFlag
                            ? DiagnosticDescriptors.CustomFlagParsingMethodMatchArgFlagParsingDelegate
                            : DiagnosticDescriptors.CustomParsingMethodMatchArgParsingDelegate,
                            Syntax);
                    }
                    parserInfo.MethodParserInputKind = inputKind;
                    break;
                case ParserInfoProvider.ParserKind.Struct:
                    if (!parserInfo.StructSymbol.IsValueType) {
                        parsedType = default!;
                        nullableNotMatched = default;
                        return DiagnosticFactory.Create(
                            DiagnosticDescriptors.CustomTypeParserShouldBeValueType,
                            Syntax);
                    }
                    if (!ValidationHelper.IsValidParserType(SemanticModel, parserInfo.StructSymbol, assignedType, isFlag, out parsedType!, out nullableNotMatched)) {
                        return DiagnosticFactory.Create(isFlag
                            ? DiagnosticDescriptors.CustomFlagParserShouldImplsIArgsFlagParser
                            : DiagnosticDescriptors.CustomParserShouldImplsIArgParser,
                            Syntax);
                    }
                    break;
                default: // Invalid or Implicit
                    throw new InvalidOperationException();
            }
            return null;
        }

        Result<IParameterData, Diagnostic> GetImplicitParserParameterData()
        {
            switch (ParameterKind) {
                // Not marked with attribute
                case ParameterKind.Invalid: {
                    var implicitParameterKind = EnumHelper.GetImplicitParameterKind(Symbol.Type);
                    return implicitParameterKind switch {
                        ImplicitParameterKind.Boolean => new FlagParameterData(this) {
                            ParserInfo = new ParserInfoProvider(implicitParameterKind),
                        },
                        ImplicitParameterKind.SpanParsable or
                        ImplicitParameterKind.Enum => new OptionParameterData(this) {
                            ParserInfo = new ParserInfoProvider(implicitParameterKind),
                        },
                        _ => DiagnosticFactory.Create(
                            DiagnosticDescriptors.ParameterNoImplicitParser,
                            Syntax),
                    };
                }

                // In non-invalid case, _attribute is not null
                case ParameterKind.Flag: {
                    var implicitParameterKind = EnumHelper.GetImplicitParameterKind(Symbol.Type);
                    return implicitParameterKind switch {
                        ImplicitParameterKind.Boolean => new FlagParameterData(this) {
                            ParserInfo = new ParserInfoProvider(implicitParameterKind),
                            Alias = _attribute!.GetConstructorArgument<string>(Literals.FlagAttribute_Alias_CtorParameterIndex),
                            Name = _attribute!.GetNamedArgument<string>(Literals.FlagAttribute_Name_PropertyIdentifier)!,
                        },
                        _ => DiagnosticFactory.Create(
                            DiagnosticDescriptors.ParameterNoImplicitParser,
                            Syntax),
                    };
                }

                case ParameterKind.Option: {
                    var implicitParameterKind = EnumHelper.GetImplicitParameterKind(Symbol.Type);
                    if (implicitParameterKind is ImplicitParameterKind.Invalid) {
                        return DiagnosticFactory.Create(
                            DiagnosticDescriptors.ParameterNoImplicitParser,
                            Syntax);
                    }

                    if (implicitParameterKind is ImplicitParameterKind.Boolean)
                        implicitParameterKind = ImplicitParameterKind.SpanParsable;
                    return new OptionParameterData(this) {
                        ParserInfo = new ParserInfoProvider(implicitParameterKind),
                        Alias = _attribute!.GetConstructorArgument<string>(Literals.OptionAttribute_Alias_CtorParameterIndex)!,
                        Name = _attribute!.GetNamedArgument<string>(Literals.OptionAttribute_Name_PropertyIdentifier)!,
                        Required = _attribute!.GetNamedArgument<bool>(Literals.OptionAttribute_Required_PropertyIdentifier)!,
                    };
                }

                case ParameterKind.Value: {
                    var implicitParameterKind = EnumHelper.GetImplicitParameterKind(Symbol.Type);
                    if (implicitParameterKind is ImplicitParameterKind.Invalid) {
                        return DiagnosticFactory.Create(
                            DiagnosticDescriptors.ParameterNoImplicitParser,
                            Syntax);
                    }

                    if (implicitParameterKind is ImplicitParameterKind.Boolean)
                        implicitParameterKind = ImplicitParameterKind.SpanParsable;
                    return new ValueParameterData(this) {
                        ParserInfo = new ParserInfoProvider(implicitParameterKind),
                        Required = _attribute!.GetNamedArgument<bool>(Literals.ValueAttribute_Required_PropertyIdentifier),
                    };
                }

                case ParameterKind.MultiValue: {
                    var collectionType = ValidationHelper.ValidateMultiCollectionType(Symbol.Type, out var elementType);
                    if (collectionType is MultiValueCollectionType.Invalid) {
                        return DiagnosticFactory.Create(
                            DiagnosticDescriptors.ParameterNoImplicitParser,
                            Syntax);
                    }

                    var implicitParameterKind = EnumHelper.GetImplicitParameterKind(elementType);
                    if (implicitParameterKind is ImplicitParameterKind.Invalid) {
                        return DiagnosticFactory.Create(
                            DiagnosticDescriptors.ParameterNoImplicitParser,
                            Syntax);
                    }

                    if (implicitParameterKind is ImplicitParameterKind.Boolean)
                        implicitParameterKind = ImplicitParameterKind.SpanParsable;
                    return new MultiValueParameterData(this) {
                        ParserInfo = new ParserInfoProvider(implicitParameterKind),
                        ResultTypeSymbol = elementType,
                        MaxCount = _attribute!.GetConstructorArgument<int>(Literals.MultiValueAttribute_MaxCount_CtorParameterIndex),
                        Required = _attribute!.GetNamedArgument<bool>(Literals.MultiValueAttribute_Required_PropertyIdentifier),
                        CollectionType = collectionType,
                    };
                }

                default:
                    throw new InvalidOperationException();
            }
        }
    }

    public Diagnostic? ValidateRequiredParameterNullableAnnotation()
    {
        if (ParameterData is not IRequiredParameterData { Required: false } or MultiValueParameterData ||
            Symbol.Type.IsCanBeDefault()
            ) {
            return null;
        }

        return DiagnosticFactory.Create(
                DiagnosticDescriptors.NotRequiredParameterMayBeDefault,
                Syntax.Type!);
    }
}
