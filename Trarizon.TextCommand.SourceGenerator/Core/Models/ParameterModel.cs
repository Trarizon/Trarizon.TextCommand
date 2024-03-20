using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
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
    public bool IsValid => ParameterData is not null;

    public SemanticModel SemanticModel => Executor.SemanticModel;

    public ExecutorModel Executor { get; } = executor;

    public required ParameterSyntax Syntax { get; init; }
    public required IParameterSymbol Symbol { get; init; }

    private AttributeData? _attribute;

    // Data

    /// <summary>
    /// Set in <see cref="ValidateSingleAttribute"/>
    /// </summary>
    public ParameterKind ParameterKind { get; private set; }

    /// <summary>
    /// Not null if <see cref="IsValid"/><br/>
    /// Set in <see cref="ValidateParameterData"/>
    /// </summary>
    public IParameterData? ParameterData { get; private set; }

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

    public IEnumerable<Diagnostic> ValidateParameterData()
    {
        // Debug.Assert(_attribute is null || ParameterKind != ParameterKind.Invalid);

        var (parameter, diagnostic) = _attribute is null
            ? GetImplicitParserParameterData()
            : GetInputParameterData();

        if (parameter is not null) {
            ParameterData = parameter;
        }
        return diagnostic ?? [];

        (IParameterData? Parameter, IEnumerable<Diagnostic>? Diagnostic) GetInputParameterData()
        {
            ParserInfoProvider memberParserInfo;

            var memberParserAttrArg = _attribute.GetNamedArgument<string>(Literals.ParameterAttribute_Parser_PropertyIdentifier);
            var typeParserAttrArg = _attribute.GetNamedArgument<ITypeSymbol>(Literals.ParameterAttribute_ParserType_PropertyIdentifier);

            switch (memberParserAttrArg, typeParserAttrArg) {
                case (null, null):
                    // use implicit parser
                    return GetImplicitParserParameterData();

                case (not null, not null):
                    return (null, DiagnosticFactory.Create(
                        DiagnosticDescriptors.DoNotSpecifyBothParserAndParserType,
                        Syntax).SingletonCollection());

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
                    memberParserAttrArg).SingletonCollection());

            var (p, d) = GetExplicitParserParameterData(memberParserInfo);
            return (p, d?.SingletonCollection());
        }

        (IInputParameterData? Parameter, Diagnostic? Diagnostic) GetExplicitParserParameterData(ParserInfoProvider parserInfo)
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
                        Required = _attribute.GetNamedArgument<bool>(Literals.IRequiredParameterAttribute_Required_PropertyIdentifier),
                    }, nullableNotMatched ? NullableNotMatchedError() : null);
                }

                case ParameterKind.Value: {
                    if (ValidateParser(ref parserInfo, Symbol.Type, false, out var parsedType, out var nullableNotMatched) is { } err) {
                        return (null, err);
                    }
                    return (new ValueParameterData(this) {
                        ParserInfo = parserInfo,
                        ParsedTypeSymbol = parsedType,
                        Required = _attribute.GetNamedArgument<bool>(Literals.IRequiredParameterAttribute_Required_PropertyIdentifier),
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
                        Required = _attribute.GetNamedArgument<bool>(Literals.IRequiredParameterAttribute_Required_PropertyIdentifier),
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

        (IParameterData? Parameter, IEnumerable<Diagnostic>? Diagnostic) GetImplicitParserParameterData()
        {
            switch (ParameterKind) {
                // Not marked with attribute
                case ParameterKind.Invalid: {
                    var implicitParameterKind = EnumHelper.GetImplicitParameterKind(Symbol.Type);
                    return implicitParameterKind switch {
                        ImplicitParameterKind.Boolean => (new FlagParameterData(this) {
                            ParserInfo = new ParserInfoProvider(implicitParameterKind),
                        }, null),
                        ImplicitParameterKind.SpanParsable or
                        ImplicitParameterKind.Enum => (new OptionParameterData(this) {
                            ParserInfo = new ParserInfoProvider(implicitParameterKind),
                        }, null),
                        _ => (null, DiagnosticFactory.Create(
                            DiagnosticDescriptors.ParameterNoImplicitParser,
                            Syntax).SingletonCollection()),
                    };
                }

                // In non-invalid case, _attribute is not null
                case ParameterKind.Flag: {
                    var implicitParameterKind = EnumHelper.GetImplicitParameterKind(Symbol.Type);
                    return implicitParameterKind switch {
                        ImplicitParameterKind.Boolean => (new FlagParameterData(this) {
                            ParserInfo = new ParserInfoProvider(implicitParameterKind),
                            Alias = _attribute!.GetConstructorArgument<string>(Literals.FlagAttribute_Alias_CtorParameterIndex),
                            Name = _attribute!.GetNamedArgument<string>(Literals.FlagAttribute_Name_PropertyIdentifier)!,
                        }, null),
                        _ => (null, DiagnosticFactory.Create(
                            DiagnosticDescriptors.ParameterNoImplicitParser,
                            Syntax).SingletonCollection()),
                    };
                }

                case ParameterKind.Option: {
                    var implicitParameterKind = EnumHelper.GetImplicitParameterKind(Symbol.Type);
                    if (implicitParameterKind is ImplicitParameterKind.Invalid) {
                        return (null, DiagnosticFactory.Create(
                            DiagnosticDescriptors.ParameterNoImplicitParser,
                            Syntax).SingletonCollection());
                    }

                    if (implicitParameterKind is ImplicitParameterKind.Boolean)
                        implicitParameterKind = ImplicitParameterKind.SpanParsable;
                    return (new OptionParameterData(this) {
                        ParserInfo = new ParserInfoProvider(implicitParameterKind),
                        Alias = _attribute!.GetConstructorArgument<string>(Literals.OptionAttribute_Alias_CtorParameterIndex)!,
                        Name = _attribute!.GetNamedArgument<string>(Literals.OptionAttribute_Name_PropertyIdentifier)!,
                        Required = _attribute!.GetNamedArgument<bool>(Literals.IRequiredParameterAttribute_Required_PropertyIdentifier)!,
                    }, default);
                }

                case ParameterKind.Value: {
                    var implicitParameterKind = EnumHelper.GetImplicitParameterKind(Symbol.Type);
                    if (implicitParameterKind is ImplicitParameterKind.Invalid) {
                        return (null, DiagnosticFactory.Create(
                            DiagnosticDescriptors.ParameterNoImplicitParser,
                            Syntax).SingletonCollection());
                    }

                    if (implicitParameterKind is ImplicitParameterKind.Boolean)
                        implicitParameterKind = ImplicitParameterKind.SpanParsable;
                    return (new ValueParameterData(this) {
                        ParserInfo = new ParserInfoProvider(implicitParameterKind),
                        Required = _attribute!.GetNamedArgument<bool>(Literals.IRequiredParameterAttribute_Required_PropertyIdentifier),
                    }, default);
                }

                case ParameterKind.MultiValue: {
                    var collectionType = ValidationHelper.ValidateMultiCollectionType(Symbol.Type, out var elementType);
                    if (collectionType is MultiValueCollectionType.Invalid) {
                        return (null, DiagnosticFactory.Create(
                            DiagnosticDescriptors.ParameterNoImplicitParser,
                            Syntax).SingletonCollection());
                    }

                    var implicitParameterKind = EnumHelper.GetImplicitParameterKind(elementType);
                    if (implicitParameterKind is ImplicitParameterKind.Invalid) {
                        return (null, DiagnosticFactory.Create(
                            DiagnosticDescriptors.ParameterNoImplicitParser,
                            Syntax).SingletonCollection());
                    }

                    if (implicitParameterKind is ImplicitParameterKind.Boolean)
                        implicitParameterKind = ImplicitParameterKind.SpanParsable;
                    return (new MultiValueParameterData(this) {
                        ParserInfo = new ParserInfoProvider(implicitParameterKind),
                        ResultTypeSymbol = elementType,
                        MaxCount = _attribute!.GetConstructorArgument<int>(Literals.MultiValueAttribute_MaxCount_CtorParameterIndex),
                        Required = _attribute!.GetNamedArgument<bool>(Literals.IRequiredParameterAttribute_Required_PropertyIdentifier),
                        CollectionType = collectionType,
                    }, default);
                }

                case ParameterKind.Context: {
                    var parameterName = _attribute!.GetNamedArgument<string>(Literals.ContextParameterAttribute_ParameterName_PropertyIdentifier) ?? Symbol.Name;
                    List<Diagnostic>? diags = null;
                    if (!Executor.Execution.Symbol.Parameters.TryFirst(p => p.Name == parameterName, out var contextParameter)) {
                        (diags ??= []).Add(DiagnosticFactory.Create(
                            DiagnosticDescriptors.ExecutorContextParameterNotFound_0ParameterName,
                            Syntax,
                            parameterName));
                    }
                    else {
                        if (!SemanticModel.Compilation.ClassifyCommonConversion(contextParameter.Type, Symbol.Type).IsIdentity) {
                            (diags ??= []).Add(DiagnosticFactory.Create(
                                DiagnosticDescriptors.CannotPassContextParameterForTypeDifference_0ExecutionParamType,
                                Syntax,
                                contextParameter.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
                        }
                        if (!ValidationHelper.IsParameterRefKindPassable(contextParameter.RefKind, Symbol.RefKind)) {
                            (diags ??= []).Add(DiagnosticFactory.Create(
                                DiagnosticDescriptors.CannotPassContextParameterForRefKind,
                                Syntax));
                        }
                    }

                    return (new ContextParameterData(this) {
                        ParameterName = parameterName,
                    }, diags ?? Enumerable.Empty<Diagnostic>());
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
