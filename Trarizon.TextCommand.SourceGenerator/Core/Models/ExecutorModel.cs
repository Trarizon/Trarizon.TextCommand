using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;
using Trarizon.TextCommand.SourceGenerator.Utilities.Factories;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models;
internal sealed class ExecutorModel(ExecutionModel execution, IEnumerable<AttributeData> attributeDatas)
{
    public SemanticModel SemanticModel => Execution.SemanticModel;

    public ExecutionModel Execution { get; } = execution;

    private ImmutableArray<ParameterModel> _parameters;
    public ImmutableArray<ParameterModel> Parameters
    {
        get {
            if (_parameters.IsDefault) {
                _parameters = Syntax.ParameterList.Parameters.Select(p
                    => new ParameterModel(this) {
                        Syntax = p,
                        Symbol = SemanticModel.GetDeclaredSymbol(p)!,
                    }).ToImmutableArray();
            }
            return _parameters;
        }
    }

    public required MethodDeclarationSyntax Syntax { get; init; }
    public required IMethodSymbol Symbol { get; init; }

    // Data

    private ImmutableArray<string[]> _commandPrefixes;
    public ImmutableArray<string[]> CommandPrefixes
    {
        get {
            if (_commandPrefixes.IsDefault) {
                _commandPrefixes = attributeDatas
                    .Select(attr => attr.GetConstructorArguments<string>(Literals.ExecutorAttribute_CommandPrefixes_CtorParameterIndex) ?? [])
                    .ToImmutableArray();
            }
            return _commandPrefixes;
        }
    }

    // Validation

    public Diagnostic? ValidateStaticKeyword()
    {
        if (Execution.Symbol.IsStatic && !Symbol.IsStatic) {
            return DiagnosticFactory.Create(
                DiagnosticDescriptors.ExecutorShouldBeStaticIfExecutionIs,
                Syntax.Identifier);
        }
        return null;
    }

    public Diagnostic? ValidateReturnType()
    {
        if (Execution.Symbol.ReturnsVoid)
            return null;

        if (SemanticModel.Compilation.ClassifyCommonConversion(
            Symbol.ReturnType, Execution.Symbol.ReturnType)
            .IsImplicit
            ) {
            return null;
        }

        return DiagnosticFactory.Create(
            DiagnosticDescriptors.ExecutorsReturnTypeShouldAssignableToExecutionsReturnType,
            Syntax.ReturnType);
    }

    public Diagnostic? ValidateCommandPrefixes()
    {
        bool isAllValid = CommandPrefixes
              .SelectMany(prefixes => prefixes)
              .All(ValidationHelper.IsValidCommandPrefix);

        if (isAllValid)
            return null;

        return DiagnosticFactory.Create(
            DiagnosticDescriptors.CommandPrefixCannotContainsSpaceOrLeadingWithMinus,
            Syntax.Identifier);
    }

    public IEnumerable<Diagnostic> ValidateOptionKeys()
    {
        var aliases = new Dictionary<string, bool>(Parameters.Length);
        var names = new Dictionary<string, bool>(Parameters.Length);

        foreach (var namedParameter in Parameters.OfType<INamedParameterData>()) {
            if (namedParameter.Alias is { } alias) {
                if (aliases.ContainsKey(alias)) {
                    aliases[alias] = true;
                    yield return DiagnosticFactory.Create(
                        DiagnosticDescriptors.NamedParameterAliasRepeat,
                        namedParameter.Model.Syntax);
                }
                else {
                    aliases.Add(alias, false);
                }
            }

            var name = namedParameter.Name;
            if (names.ContainsKey(name)) {
                names[name] = true;
                yield return DiagnosticFactory.Create(
                    DiagnosticDescriptors.NamedParameterNameRepeat,
                    namedParameter.Model.Syntax);
            }
            else {
                names.Add(name, false);
            }
        }
    }

    /// <summary>
    /// Requires Parameters set
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Diagnostic> ValidateValueParametersCount()
    {
        using var enumerator = Parameters.Select(p => p.ParameterData).OfType<IValueParameterData>().GetEnumerator();

        DiagnosticDescriptor? descriptor;

        int count = 0;
        while (enumerator.MoveNext()) {
            var parameter = enumerator.Current;
            parameter.Index = count;

            if (parameter.IsRest) { // Start from next, rest values are unreachable
                descriptor = DiagnosticDescriptors.ValueOrMultiValueAfterRestValueWillAlwaysDefault;
                goto Unreachable;
            }
            count = unchecked(count + parameter.MaxCount);

            if (count < 0) { // Overflow
                descriptor = DiagnosticDescriptors.ValueCountOverflow;
                goto Unreachable;
            }
        }
        yield break;

    Unreachable:
        while (enumerator.MoveNext()) {
            var parameter = enumerator.Current;
            parameter.Index = -1;
            yield return DiagnosticFactory.Create(
                descriptor,
                parameter.Model.Syntax);
        }
    }
}
