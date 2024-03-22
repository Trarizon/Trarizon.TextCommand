using Microsoft.CodeAnalysis;
using System;

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

    public readonly static DiagnosticDescriptor ExecutionMethodHasAtLeastOneParameter = new(
        "TCMD0002",
        nameof(ExecutionMethodHasAtLeastOneParameter),
        "Execution method has at least one parameter, and the first is the input",
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

    public readonly static DiagnosticDescriptor ExecutorCommandPrefixRepeatOrTruncate_0PrevExecutorName = new(
        "TCMD0005",
        nameof(ExecutorCommandPrefixRepeatOrTruncate_0PrevExecutorName),
        "Executor never called, command is matched by previous executor {0}",
        Literals.Namespace,
        DiagnosticSeverity.Error,
        true);

    public readonly static DiagnosticDescriptor MarkSingleParameterAttributes = new(
        "TCMD0006",
        nameof(MarkSingleParameterAttributes),
        "Only single parameter attribute could be marked on a parameter",
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

    public readonly static DiagnosticDescriptor CannotFindExplicitParser_0MemberName = new(
        "TCMD0008",
        nameof(CannotFindExplicitParser_0MemberName),
        @"Cannot find explicit parser field, property, or method named ""{0}"" in command type",
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
        $"Custom flag parsing method should match ArgFlagParsingDelegate",
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

    public readonly static DiagnosticDescriptor CommandPrefixCannotContainsSpaceOrLeadingWithMinus = new(
        "TCMD0016",
        nameof(CommandPrefixCannotContainsSpaceOrLeadingWithMinus),
        "Command prefix cannot contain space or leading with '-'",
        Literals.Namespace,
        DiagnosticSeverity.Error,
        true);

    public readonly static DiagnosticDescriptor ValueOrMultiValueAfterRestValueWillAlwaysDefault = new(
        "TCMD0017",
        nameof(ValueOrMultiValueAfterRestValueWillAlwaysDefault),
        "'Value' or 'MultiValue' after a 'RestValue'(MultiValue with MaxCount <= 0) will never have value",
        Literals.Namespace,
        DiagnosticSeverity.Warning,
        true);

    public readonly static DiagnosticDescriptor NotRequiredParameterMayBeDefault = new(
        "TCMD0018",
        nameof(NotRequiredParameterMayBeDefault),
        "Not required parameter may be default",
        Literals.Namespace,
        DiagnosticSeverity.Warning,
        true);

    public readonly static DiagnosticDescriptor NamedParameterAliasRepeat = new(
        "TCMD0019",
        nameof(NamedParameterAliasRepeat),
        "Parameter alias repeat",
        Literals.Namespace,
        DiagnosticSeverity.Error,
        true);

    public readonly static DiagnosticDescriptor NamedParameterNameRepeat = new(
        "TCMD0020",
        nameof(NamedParameterNameRepeat),
        "Parameter name repeat",
        Literals.Namespace,
        DiagnosticSeverity.Error,
        true);

    public readonly static DiagnosticDescriptor ParsedArgumentMaybeNull = new(
        "TCMD0021",
        nameof(ParsedArgumentMaybeNull),
        "Parsed argument may be null while parameter is not null",
        Literals.Namespace,
        DiagnosticSeverity.Warning,
        true);

    public readonly static DiagnosticDescriptor CannotFindErrorHandlerMethod_0RequiredMethodName = new(
        "TCMD0022",
        nameof(CannotFindErrorHandlerMethod_0RequiredMethodName),
        "Cannot find valid error handler method {0}",
        Literals.Namespace,
        DiagnosticSeverity.Error,
        true,
        description:"""
        An error hander should match the signature: void|TReturn Handler((in) ArgResultErrors, [string]).
        TReturn is assignable to return type of execution method, 2nd parameter is optional.
        """);

    public readonly static DiagnosticDescriptor CannotAccessMethod_0MethodName = new(
        "TCMD0023",
        nameof(CannotAccessMethod_0MethodName),
        "The method {0} is not accessable from current block",
        Literals.Namespace,
        DiagnosticSeverity.Error,
        true);

    public readonly static DiagnosticDescriptor CustomTypeParserShouldBeValueType = new(
        "TCMD0024",
        nameof(CustomTypeParserShouldBeValueType),
        "Custom type parser should be value type, because we use default(Parser) as parser",
        Literals.Namespace,
        DiagnosticSeverity.Error,
        true);

    public readonly static DiagnosticDescriptor DoNotSpecifyBothParserAndParserType = new(
        "TCMD0025",
        nameof(DoNotSpecifyBothParserAndParserType),
        "Do not specify both Parser and ParserType at same time",
        Literals.Namespace,
        DiagnosticSeverity.Error,
        true);

    public readonly static DiagnosticDescriptor ValueCountOverflow = new(
        "TCMD0026",
        nameof(ValueCountOverflow),
        "Total count of Value parameter is over int.MaxValue, this value will never have value",
        Literals.Namespace,
        DiagnosticSeverity.Warning,
        true);

    public readonly static DiagnosticDescriptor ExecutionInputParameterInvalid = new(
        "TCMD0027",
        nameof(ExecutionInputParameterInvalid),
        "Input type should be string or ReadOnlySpan<char> is no custom matcher",
        Literals.Namespace,
        DiagnosticSeverity.Error,
        true);

    public readonly static DiagnosticDescriptor ExecutorContextParameterNotFound_0ParameterName = new(
        "TCMD0028",
        nameof(ExecutorContextParameterNotFound_0ParameterName),
        "Cannot find parameter {0} in execution method",
        Literals.Namespace,
        DiagnosticSeverity.Error,
        true);

    public readonly static DiagnosticDescriptor CannotPassContextParameterForTypeDifference_0ExecutionParamType = new(
        "TCMD0029",
        nameof(CannotPassContextParameterForTypeDifference_0ExecutionParamType),
        "Cannot pass context parameter, parameter type should be {0}",
        Literals.Namespace,
        DiagnosticSeverity.Error,
        true);

    public readonly static DiagnosticDescriptor CannotPassContextParameterForRefKind = new(
        "TCMD0030",
        nameof(CannotPassContextParameterForRefKind),
        "Cannot pass due to ref kind incompatible",
        Literals.Namespace,
        DiagnosticSeverity.Error,
        true);
}
