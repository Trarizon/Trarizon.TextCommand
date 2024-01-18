﻿using Microsoft.CodeAnalysis;

namespace Trarizon.TextCommand.SourceGenerator.ConstantValues;
internal static class DiagnosticDescriptors
{
    public readonly static DiagnosticDescriptor RequiresLangVersionCSharp12 = new(
        "TCMD0001",
        nameof(RequiresLangVersionCSharp12),
        "Trarizon.TextCommand requires language version c#12",
        Literals.Namespace,
        DiagnosticSeverity.Warning,
        true);

    public readonly static DiagnosticDescriptor ExecutionMethodOnlyHasOneParameter = new(
        "TCMD0002",
        nameof(ExecutionMethodOnlyHasOneParameter),
        "Execution method can have only one parameter, which is the input",
        Literals.Namespace,
        DiagnosticSeverity.Error,
        true);

    public readonly static DiagnosticDescriptor ExecutionMethodReturnTypeShouldBeNullable = new(
        "TCMD0003",
        nameof(ExecutionMethodReturnTypeShouldBeNullable),
        "Return value of execution may be default",
        Literals.Namespace,
        DiagnosticSeverity.Warning,
        true);

    public readonly static DiagnosticDescriptor ExecutorsReturnTypeShouldAssignableToExecutionsReturnType = new(
        "TCMD0004",
        nameof(ExecutorsReturnTypeShouldAssignableToExecutionsReturnType),
        "Return type of executor should be assignable to return type of execution",
        Literals.Namespace,
        DiagnosticSeverity.Error,
        true);

    public readonly static DiagnosticDescriptor ExecutorCommandPrefixRepeatOrTruncate_1 = new(
        "TCMD0005",
        nameof(ExecutorsReturnTypeShouldAssignableToExecutionsReturnType),
        "Executor never called, command is matched by previous executor {0}",
        Literals.Namespace,
        DiagnosticSeverity.Error,
        true);

    public readonly static DiagnosticDescriptor MarkSingleParameterAttributes = new(
        "TCMD0006",
        nameof(MarkSingleParameterAttributes),
        "Only single parameter attribute could be marked on aa parameter",
        Literals.Namespace,
        DiagnosticSeverity.Error,
        true);

    public readonly static DiagnosticDescriptor ParameterNoImplicitParser = new(
        "TCMD0007",
        nameof(ParameterNoImplicitParser),
        "Cannot implicit select parser, please indicate parser explicitly",
        Literals.Namespace,
        DiagnosticSeverity.Error,
        true);

    public readonly static DiagnosticDescriptor CannotFindExplicitParser = new(
        "TCMD0008",
        nameof(CannotFindExplicitParser),
        "Cannot find explicit parser field, property, or method in command type",
        Literals.Namespace,
        DiagnosticSeverity.Error,
        true);

    public readonly static DiagnosticDescriptor CustomParserShouldImplsIArgParser = new(
        "TCMD0009",
        nameof(CustomParserShouldImplsIArgParser),
        $"Custom parser should implements {Literals.IArgParser_TypeName} (or {Literals.IArgFlagParser_TypeName} for flag), and match the type of parameter",
        Literals.Namespace,
        DiagnosticSeverity.Error,
        true);

    public readonly static DiagnosticDescriptor CustomFlagParserShouldImplsIArgsFlagParser = new(
        "TCMD0010",
        nameof(CustomFlagParserShouldImplsIArgsFlagParser),
        $"Custom flag parser should implements {Literals.IArgFlagParser_TypeName}, and match the type of parameter",
        Literals.Namespace,
        DiagnosticSeverity.Error,
        true);

    public readonly static DiagnosticDescriptor InvalidMultiValueCollectionType = new(
        "TCMD0011",
        nameof(InvalidMultiValueCollectionType),
        "Type {0} cannot be recognized as multi-value collection type, use Array, List, Span, ROS or Enumerable",
        Literals.Namespace,
        DiagnosticSeverity.Error,
        true);

    public readonly static DiagnosticDescriptor MultiValueParserCannotBeFlagParser = new(
        "TCMD0012",
        nameof(InvalidMultiValueCollectionType),
        "Custom parser of multi-value cannot be flag parser, items are treat as options or values",
        Literals.Namespace,
        DiagnosticSeverity.Error,
        true);

    public readonly static DiagnosticDescriptor ExecutionMethodShouldBePartial = new(
        "TCMD0012",
        nameof(ExecutionMethodShouldBePartial),
        "Execution method should be partial",
        Literals.Namespace,
        DiagnosticSeverity.Error,
        true);

    public readonly static DiagnosticDescriptor CustomFlagParsingMethodMatchArgFlagParsingDelegate = new(
        "TCMD0013",
        nameof(CustomFlagParsingMethodMatchArgFlagParsingDelegate),
        $"Custom flag parsing method shoule match ArgFlagParsingDelegate",
        Literals.Namespace,
        DiagnosticSeverity.Error,
        true);

    public readonly static DiagnosticDescriptor CustomParsingMethodMatchArgParsingDelegate = new(
        "TCMD0014",
        nameof(CustomParsingMethodMatchArgParsingDelegate),
        $"Custom flag parsing method shoule match ArgParsingDelegate",
        Literals.Namespace,
        DiagnosticSeverity.Error,
        true);

    public readonly static DiagnosticDescriptor ExecutorShouldBeStaticIfExecutionIs = new(
        "TCMD0015",
        nameof(ExecutorShouldBeStaticIfExecutionIs),
        $"Static execution cannot call non-static executor",
        Literals.Namespace,
        DiagnosticSeverity.Error,
        true);
}