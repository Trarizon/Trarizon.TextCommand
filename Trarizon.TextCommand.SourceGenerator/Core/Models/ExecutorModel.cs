using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas.Markers;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;
using Trarizon.TextCommand.SourceGenerator.Utilities.Factories;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models;
internal sealed class ExecutorModel(ExecutionModel execution, IEnumerable<AttributeData> attributes)
{
    public SemanticModel SemanticModel => Execution.SemanticModel;

    public ExecutionModel Execution { get; } = execution;

    private IReadOnlyList<ParameterModel>? _parameters;
    public IReadOnlyList<ParameterModel> Parameters => _parameters ??= DefinationSymbol.Parameters
        .Select(param => new ParameterModel(this) { Symbol = param, })
        .ToArray();

    public required IMethodSymbol Symbol { get; init; }
    private IMethodSymbol DefinationSymbol => Symbol.PartialDefinitionPart ?? Symbol;

    private MethodDeclarationSyntax? _definitionSyntax;
    public MethodDeclarationSyntax DefinitionSyntax => _definitionSyntax
        ??= (MethodDeclarationSyntax)DefinationSymbol.DeclaringSyntaxReferences[0].GetSyntax();

    private readonly IEnumerable<AttributeData> _attributes = attributes;

    // Data

    private IReadOnlyList<ImmutableArray<string>>? _commandPrefixes;
    public IReadOnlyList<ImmutableArray<string>> CommandPrefixes => _commandPrefixes
        ??= _attributes
            .Select(attr => attr.GetConstructorArguments<string>(Literals.ExecutorAttribute_CommandPrefixes_CtorParameterIndex))
            .ToList();

    public bool IsMultipleExecutor => CommandPrefixes.Count > 1;

    public IEnumerable<Diagnostic?> Validate() => [
        ValidateStaticKeyword(),
        ValidateReturnType(),
        ValidateCommandPrefixes(),
        ..Parameters.SelectMany(p => p.Validate()),
        ..ValidateOptionNames(),
        ..ValidateValueParametersCount(),
        ];

    public Diagnostic? ValidateStaticKeyword()
    {
        if (Execution.Symbol.IsStatic && !Symbol.IsStatic) {
            return DiagnosticFactory.Create(
                DiagnosticDescriptors.ExecutorShouldBeStaticIfExecutionIs,
                DefinitionSyntax.Identifier);
        }

        return null;
    }

    public Diagnostic? ValidateReturnType()
    {
        if (Execution.Symbol.ReturnsVoid)
            return null;

        if (Symbol.ReturnType.IsImplicitAssignableTo(Execution.Symbol.ReturnType, SemanticModel, out _))
            return null;

        return DiagnosticFactory.Create(
            DiagnosticDescriptors.ExecutorsReturnTypeShouldAssignableToExecutionsReturnType,
            DefinitionSyntax.Identifier);
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
            DefinitionSyntax.Identifier);
    }

    /// <summary>
    /// This will not validate invalid parameter
    /// </summary>
    public IEnumerable<Diagnostic> ValidateOptionNames()
    {
        var aliases = new HashSet<string>();
        var names = new HashSet<string>();

        foreach (var namedParameter in Parameters.Select(p => p.Data).OfType<INamedParameterData>()) {
            var name = namedParameter.Name;
            if (!names.Add(name)) {
                yield return DiagnosticFactory.Create(
                    DiagnosticDescriptors.NamedParameterNameRepeat,
                    namedParameter.Model.Syntax);
            }

            if (namedParameter.Alias is null)
                continue;

            var alias = namedParameter.Alias;
            if (!aliases.Add(alias)) {
                yield return DiagnosticFactory.Create(
                    DiagnosticDescriptors.NamedParameterAliasRepeat,
                    namedParameter.Model.Syntax);
            }
        }
    }

    /// <summary>
    /// This will not validate invalid parameter
    /// </summary>
    public IEnumerable<Diagnostic> ValidateValueParametersCount()
    {
        using var enumerator = Parameters.Select(p => p.Data).OfType<IPositionalParameterDataMutable>().GetEnumerator();

        DiagnosticDescriptor? descriptor;
        int count = 0;

        while (enumerator.MoveNext()) {
            var parameter = enumerator.Current;
            parameter.StartIndex = count;

            if (parameter.IsRest) {
                descriptor = DiagnosticDescriptors.ValueOrMultiValueAfterRestValueWillAlwaysDefault;
                goto Unreachable;
            }
            if ((long)count + parameter.MaxCount > int.MaxValue) {
                descriptor = DiagnosticDescriptors.ValueCountOverflow;
                goto Unreachable;
            }
        }
        yield break;

    Unreachable:
        while (enumerator.MoveNext()) {
            var parameter = enumerator.Current;
            parameter.StartIndex = -1;
            yield return DiagnosticFactory.Create(
                descriptor,
                parameter.Model.Syntax);
        }
    }
}
