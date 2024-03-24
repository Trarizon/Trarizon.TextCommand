using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Tags;
using Trarizon.TextCommand.SourceGenerator.Utilities;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;
using Trarizon.TextCommand.SourceGenerator.Utilities.Factories;
using ErrorHandlerTuple = (
    Trarizon.TextCommand.SourceGenerator.Core.Tags.ErrorHandlerKind Kind,
    Microsoft.CodeAnalysis.IMethodSymbol Symbol);
using InputParameterTuple = (
    Trarizon.TextCommand.SourceGenerator.Core.Tags.InputParameterKind Kind,
    Microsoft.CodeAnalysis.IParameterSymbol Symbol);
using MatcherSelectorTuple = (
    Microsoft.CodeAnalysis.IMethodSymbol Symbol,
    Trarizon.TextCommand.SourceGenerator.Core.Tags.InputParameterKind ReturnInputParameterKind);

namespace Trarizon.TextCommand.SourceGenerator.Core.Models;
internal sealed class ExecutionModel
{
    public SemanticModel SemanticModel { get; }

    public CommandModel Command { get; }

    public IReadOnlyList<ExecutorModel>? _executors;
    public IReadOnlyList<ExecutorModel> Executors => _executors
        ??= Command.Symbol.GetMembers()
            .OfType<IMethodSymbol>()
            .WhereSelect(method =>
            {
                if (SymbolEqualityComparer.Default.Equals(method, Symbol))
                    return default;
                var attrs = method.GetAttributes()
                    .Where(attr => attr.AttributeClass.MatchDisplayString(Literals.ExecutorAttribute_TypeName));
                if (!attrs.Any())
                    return default;

                return WrapperUtils.Optional(new ExecutorModel(this, attrs) {
                    Symbol = method,
                });
            })
            .ToList();

    public MethodDeclarationSyntax Syntax { get; }
    public IMethodSymbol Symbol { get; }

    private readonly AttributeData _attribute;

    public ExecutionModel(GeneratorAttributeSyntaxContext context)
    {
        SemanticModel = context.SemanticModel;
        _attribute = context.Attributes[0];
        Syntax = (MethodDeclarationSyntax)context.TargetNode;
        Symbol = (IMethodSymbol)context.TargetSymbol;

        Command = new CommandModel(this) {
            Syntax = Syntax.Ancestors().OfType<TypeDeclarationSyntax>().First(),
            Symbol = Symbol.ContainingType,
        };
    }

    // Data

    public InputParameterTuple InputParameter { get; private set; }

    /// <summary>
    /// Invalid if <see cref="InputParameter"/> is not <see cref="InputParameterKind.CustomMatcher"/>
    /// </summary>
    public MatcherSelectorTuple CustomMatcherSelector { get; private set; }

    /// <summary>
    /// Not null
    /// Set in <see cref="ValidateCommandNames"/>
    /// </summary>
    private ImmutableArray<string> _commandNames;
    public ImmutableArray<string> CommandNames
    {
        get {
            if (_commandNames.IsDefault) {
                _commandNames = _attribute.GetConstructorArguments<string>(Literals.ExecutionAttribute_CommandNames_CtorParameterIndex);
                if (_commandNames.IsDefault)
                    _commandNames = ImmutableArray<string>.Empty;
            }
            return _commandNames;
        }
    }

    public ErrorHandlerTuple CustomErrorHandler { get; private set; }

    // Validate

    public IEnumerable<Diagnostic?> Validate() => [
        ..ValidateInputParameter(),
        ValidateReturnType(),
        ValidateErrorHandler(),
        ValidateErrorHandler(),
        ..ValidateExecutorsCommandPredixes(),
        ..Executors.SelectMany(exec => exec.Validate()),
        ];

    /// <remarks>
    /// 有一个以上参数
    /// matcher不符合要求时，warning，并按默认情况生成
    /// </remarks>
    public IEnumerable<Diagnostic> ValidateInputParameter()
    {
        if (Symbol.Parameters is not [var inputParameter, ..]) {
            yield return DiagnosticFactory.Create(
                DiagnosticDescriptors.ExecutionMethodHasAtLeastOneParameter,
                Syntax.Identifier);
            yield break;
        }

        var matcherSelectorName = _attribute.GetNamedArgument<string>(Literals.ExecutionAttribute_MatcherSelector_PropertyIdentifier);
        if (matcherSelectorName is null) {
            goto NoCustomMatcher;
        }

        InputParameterKind matcherReturnParameterKind = default;
        var matcherSelector = Command.Symbol.EnumerateByWhileNotNull(type => type.BaseType)
            .SelectMany(type => type.GetMembers(matcherSelectorName))
            .OfType<IMethodSymbol>()
            .FirstOrDefault(method =>
            {
                matcherReturnParameterKind = ValidationHelper.ValidateCustomMatcherSelector(method, SemanticModel, inputParameter.Type);
                return matcherReturnParameterKind is not InputParameterKind.Invalid;
            });

        if (matcherSelector is null) {
            yield return DiagnosticFactory.Create(
                DiagnosticDescriptors.CannotFindValidCustomMatcherSelectorMethod_0RequiredMethodName,
                Syntax.Identifier,
                matcherSelectorName);
            // When custom matcher invalid, we use default matcher
            goto NoCustomMatcher;
        }

        // CustomMatcher:

        InputParameter = (InputParameterKind.CustomMatcher, inputParameter);
        CustomMatcherSelector = (matcherSelector, matcherReturnParameterKind);
        yield break;

    NoCustomMatcher:

        var inputParamKind = ValidationHelper.ValidateNonCustomInputParameterKind(inputParameter.Type);
        if (inputParamKind is InputParameterKind.Invalid) {
            yield return DiagnosticFactory.Create(
                     DiagnosticDescriptors.ExecutionInputParameterInvalid,
                     Syntax.ParameterList.Parameters[0]);
            yield break;
        }
        else {
            InputParameter = (inputParamKind, inputParameter);
        }
    }

    /// <remarks>
    /// 应当defaultable
    /// </remarks>
    public Diagnostic? ValidateReturnType()
    {
        if (Symbol.ReturnsVoid)
            return null;

        if (Symbol.ReturnType.IsMayBeDefault())
            return null;

        return DiagnosticFactory.Create(
            DiagnosticDescriptors.ExecutionMethodReturnTypeShouldBeNullable,
            Syntax.ReturnType);
    }

    public Diagnostic? ValidateCommandNames()
    {
        if (CommandNames.IsEmpty)
            return null;

        if (CommandNames.All(ValidationHelper.IsValidCommandPrefix))
            return null;

        return DiagnosticFactory.Create(
            DiagnosticDescriptors.CommandPrefixCannotContainsSpaceOrLeadingWithMinus,
            Syntax.Identifier);
    }

    public Diagnostic? ValidateErrorHandler()
    {
        var errHandlerName = _attribute.GetNamedArgument<string>(Literals.ExecutionAttribute_ErrorHandler_PropertyIdentifier);
        if (errHandlerName is null)
            return null;

        var (errHandlerKind, errHandlerMethod) = Command.Symbol.EnumerateByWhileNotNull(type => type.BaseType)
            .Select(type => type.GetMembers(errHandlerName)
                .OfType<IMethodSymbol>()
                .FirstByMaxPriorityOrDefault(ErrorHandlerKind.WithExecutorName,
                    method => ValidationHelper.ValidateErrorHandler(method, SemanticModel, Symbol.ReturnType)))
            .FirstOrDefault(handler => handler.Value is not null);

        Debug.Assert(errHandlerKind is ErrorHandlerKind.Invalid == errHandlerMethod is null);

        if (errHandlerMethod is null) {
            return DiagnosticFactory.Create(
               DiagnosticDescriptors.CannotFindValidErrorHandlerMethod_0RequiredMethodName,
               Syntax.Identifier,
               errHandlerName);
        }

        CustomErrorHandler = (errHandlerKind, errHandlerMethod);
        // Is method not accessible, compiler will warns.
        return null;
    }

    public IEnumerable<Diagnostic> ValidateExecutorsCommandPredixes()
    {
        return CrossSelf(Executors
            .SelectMany(e => e.CommandPrefixes, (exec, cmd) => (exec, cmd))
            .ToList())
            .Where(tuple =>
            {
                var (former, latter) = (tuple.Former.cmd, tuple.Latter.cmd);
                if (former.Length > latter.Length)
                    return false;
                return former.AsSpan().SequenceEqual(latter.AsSpan(0, former.Length));
            })
            .Select(tuple =>
            {
                return DiagnosticFactory.Create(
                    DiagnosticDescriptors.ExecutorCommandPrefixRepeatOrTruncate_0PrevExecutorName,
                    tuple.Latter.exec.DefinitionSyntax.Identifier,
                    tuple.Former.exec.Symbol.Name);
            });

        static IEnumerable<(T Former, T Latter)> CrossSelf<T>(IList<T> list)
        {
            for (int i = 0; i < list.Count; i++) {
                for (int j = i + 1; j < list.Count; j++) {
                    yield return (list[i], list[j]);
                }
            }
        }
    }
}
