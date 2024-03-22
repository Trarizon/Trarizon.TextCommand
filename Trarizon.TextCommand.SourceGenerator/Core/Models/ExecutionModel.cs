using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Tags;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;
using Trarizon.TextCommand.SourceGenerator.Utilities.Factories;

using MatcherSelectorTuple = (
    Trarizon.TextCommand.SourceGenerator.Core.Tags.InputParameterType ReturnParameterType,
    Microsoft.CodeAnalysis.IMethodSymbol? MethodSymbol);

namespace Trarizon.TextCommand.SourceGenerator.Core.Models;
internal sealed class ExecutionModel(CommandModel command, AttributeData attributeData)
{
    public SemanticModel SemanticModel => Command.SemanticModel;

    public CommandModel Command { get; } = command;

    private IReadOnlyList<ExecutorModel>? _executors;
    public IReadOnlyList<ExecutorModel> Executors => _executors ??= Command.GetExecutorModels().ToList();

    public required MethodDeclarationSyntax Syntax { get; init; }
    public required IMethodSymbol Symbol { get; init; }

    // Data

    /// <summary>
    /// Set by <see cref="ValidateParameter"/>
    /// </summary>
    public InputParameterType InputParameterType { get; private set; }

    private Optional<string?> _commandName;
    public string? CommandName
    {
        get {
            if (!_commandName.HasValue) {
                _commandName = attributeData.GetConstructorArgument<string>(Literals.ExecutionAttribute_CommandName_CtorParameterIndex);
            }
            return _commandName.Value;
        }
    }

    /// <summary>
    /// Set by <see cref="ValidateErrorHandler"/>
    /// </summary>
    public IMethodSymbol? ErrorHandler { get; private set; }

    /// <summary>
    /// Set by <see cref="ValidateParameter"/>
    /// </summary>
    public MatcherSelectorTuple MatcherSelector { get; private set; }

    // Validations

    public Diagnostic? ValidateParameter()
    {
        if (Symbol.Parameters is not [{ Type: var inputParameterType }, ..]) {
            return DiagnosticFactory.Create(
                DiagnosticDescriptors.ExecutionMethodHasAtLeastOneParameter,
                Syntax.Identifier);
        }

        var matcherSelectorName = attributeData.GetNamedArgument<string>(Literals.ExecutionAttribute_MatcherSelector_PropertyIdentifier);
        if (matcherSelectorName is null) {
            MatcherSelector = default;
            return null; ;
        }

        InputParameterType matcherReturnParameterType = default;
        var matcherSelector = Command.Symbol.EnumerateByWhileNotNull(type => type.BaseType)
            .SelectMany(type => type.GetMembers(matcherSelectorName))
            .OfType<IMethodSymbol>()
            .FirstOrDefault(method => ValidationHelper.IsValidMatcherSelector(SemanticModel, method, inputParameterType, out var returnTypeParameterType));

        if (matcherSelector is null) {
            InputParameterType = ValidationHelper.ValidateInputParameterType(inputParameterType);

            if (InputParameterType is InputParameterType.Invalid) {
                return DiagnosticFactory.Create(
                    DiagnosticDescriptors.ExecutionInputParameterInvalid,
                    Syntax.ParameterList.Parameters[0]);
            }

            return null;
        }
        else {
            InputParameterType = InputParameterType.CustomMatcher;
            MatcherSelector = (matcherReturnParameterType, matcherSelector);

            if (matcherReturnParameterType is InputParameterType.Invalid) {
                return DiagnosticFactory.Create(
                    // TODO: new diagnostic
                    // TODO: Next: Provider
                    DiagnosticDescriptors.ExecutionInputParameterInvalid,
                    Syntax.ParameterList.Parameters[0]);
            }

            return null;
        }
    }

    public Diagnostic? ValidateReturnType()
    {
        if (Symbol.ReturnsVoid)
            return null;

        if (Symbol.ReturnType.IsCanBeDefault())
            return null;

        return DiagnosticFactory.Create(
            DiagnosticDescriptors.ExecutionMethodReturnTypeShouldBeNullable,
            Syntax.ReturnType);
    }

    public Diagnostic? ValidateCommandName()
    {
        var name = CommandName;

        if (name is null || ValidationHelper.IsValidCommandPrefix(name))
            return null;

        return DiagnosticFactory.Create(
            DiagnosticDescriptors.CommandPrefixCannotContainsSpaceOrLeadingWithMinus,
            Syntax.Identifier);
    }

    public Diagnostic? ValidateErrorHandler()
    {
        var errorHandler = attributeData.GetNamedArgument<string>(Literals.ExecutionAttribute_ErrorHandler_PropertyIdentifier);
        if (errorHandler is null)
            return null;

        var commandTypeSymbol = Command.Symbol;

        (_, var errHandlerMethod) = commandTypeSymbol.EnumerateByWhileNotNull(cur => cur.BaseType)
            .Select(type => type.GetMembers(errorHandler)
                .OfType<IMethodSymbol>()
                .FirstByPriorityOrDefault(ErrorHandlerValidationResult.TwoParameter,
                    method => ValidationHelper.IsValidErrorHandler(SemanticModel, method, Symbol.ReturnType)))
            .FirstOrDefault(handler => handler.Value is not null);

        if (errHandlerMethod is null) {
            return DiagnosticFactory.Create(
                DiagnosticDescriptors.CannotFindErrorHandlerMethod_0RequiredMethodName,
                Syntax.Identifier,
                errorHandler);
        }

        ErrorHandler = errHandlerMethod;

        // If the type is declared in base type, we need to check if it is accessible
        if (!SymbolEqualityComparer.Default.Equals(commandTypeSymbol, errHandlerMethod.ContainingType) &&
            errHandlerMethod.DeclaredAccessibility is Accessibility.Private or Accessibility.NotApplicable
            ) {
            return DiagnosticFactory.Create(
                    DiagnosticDescriptors.CannotAccessMethod_0MethodName,
                    Syntax.Identifier,
                    errHandlerMethod.Name);
        }

        return null;
    }

    public IEnumerable<Diagnostic> ValidateExecutorsCommandPrefixes()
    {
        var tmp = Executors.SelectMany(e => e.CommandPrefixes, (executor, cmd) => (executor, cmd)).ToList();

        for (int i = 0; i < tmp.Count; i++) {
            var (formerExecutor, formerCmdPrefixes) = tmp[i];
            for (int j = i + 1; j < tmp.Count; j++) {
                var (latterExecutor, latterCmdPrefixes) = tmp[j];
                if (RepeatOfTruncate(formerCmdPrefixes, latterCmdPrefixes)) {
                    yield return DiagnosticFactory.Create(
                        DiagnosticDescriptors.ExecutorCommandPrefixRepeatOrTruncate_0PrevExecutorName,
                        latterExecutor.Syntax.Identifier,
                        formerExecutor.Symbol.Name);
                }
            }
        }

        static bool RepeatOfTruncate(ImmutableArray<string> former, ImmutableArray<string> latter)
        {
            if (former.Length > latter.Length)
                return false;

            return former.AsSpan().SequenceEqual(latter.AsSpan(0, former.Length));
        }
    }
}
