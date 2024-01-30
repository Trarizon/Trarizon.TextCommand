using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models.Parameters;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;
using Trarizon.TextCommand.SourceGenerator.Utilities.Factories;
using Trarizon.TextCommand.SourceGenerator.Utilities.Toolkit;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models;
internal sealed class ExecutorModel(ExecutionModel execution, MethodDeclarationSyntax syntax, IMethodSymbol symbol, IReadOnlyList<AttributeData> attributes)
{
    public ExecutionModel Execution { get; } = execution;

    private ImmutableArray<ParameterModel> _parameters;
    public ImmutableArray<ParameterModel> Parameters
    {
        get {
            if (_parameters.IsDefault) {
                _parameters = Syntax.ParameterList.Parameters
                    .Select(p => new ParameterModel(this, p, Execution.Command.SemanticModel.GetDeclaredSymbol(p)!))
                    .ToImmutableArray();
            }
            return _parameters;
        }
    }

    public MethodDeclarationSyntax Syntax { get; } = syntax;

    public IMethodSymbol Symbol { get; } = symbol;

    private readonly IReadOnlyList<AttributeData> _attributes = attributes;

    // Data

    private string[][]? _commandPrefixes;
    public string[][] CommandPrefixes => _commandPrefixes ??= _attributes.Select(attr => attr.GetConstructorArguments<string>(Literals.ExecutorAttribute_CommandPrefixes_CtorParameterIndex) ?? []).ToArray();

    public Filter ValidateStaticKeyword()
    {
        if (Execution.Symbol.IsStatic && !Symbol.IsStatic) {
            return Filter.CreateDiagnostic(DiagnosticFactory.Create(
                DiagnosticDescriptors.ExecutorShouldBeStaticIfExecutionIs,
                Syntax.Identifier));
        }
        return Filter.Success;
    }

    public Filter ValidateReturnType()
    {
        if (Execution.Symbol.ReturnsVoid)
            return Filter.Success;

        if (Execution.Command.SemanticModel.Compilation.ClassifyCommonConversion(
            Symbol.ReturnType,
            Execution.Symbol.ReturnType).IsImplicit) {
            return Filter.Success;
        }

        return Filter.CreateDiagnostic(DiagnosticFactory.Create(
            DiagnosticDescriptors.ExecutorsReturnTypeShouldAssignableToExecutionsReturnType,
            Syntax.ReturnType));
    }

    public Filter ValidateCommandPrefixes()
    {
        foreach (var prefixes in CommandPrefixes) {
            foreach (var prefix in prefixes) {
                if (!ValidationHelper.IsValidCommandPrefix(prefix))
                    return Filter.CreateDiagnostic(DiagnosticFactory.Create(
                        DiagnosticDescriptors.CommandPrefixCannotContainsSpaceOrLeadingWithMinus,
                        Syntax.Identifier));
            }
        }

        return Filter.Success;
    }

    public Filter ValidateValueParametersAfterRestValues()
    {
        var parameters = Parameters;
        bool hasRestValues = false;

        List<Diagnostic> errors = [];
        for (int i = 0; i < parameters.Length; i++) {
            var p = parameters[i];
            if (hasRestValues) {
                if (p.ParameterKind is ParameterKind.Value or ParameterKind.MultiValue)
                    errors.Add(DiagnosticFactory.Create(
                        DiagnosticDescriptors.ValueOrMultiValueAfterRestValueWillAlwaysDefault,
                        p.Syntax));
            }
            else if (p.ParameterData is MultiValueParameterData { IsRest: true })
                hasRestValues = true;
        }

        if (errors.Count > 0)
            return Filter.CreateDiagnostic(errors);
        else
            return Filter.Success;
    }

    public Filter ValidateOptionKeys()
    {
        Dictionary<string, INamedParameterData> aliases = new(Parameters.Length);
        Dictionary<string, INamedParameterData> names = new(Parameters.Length);

        List<INamedParameterData>? warnedAliases = null;
        List<INamedParameterData>? warnedNames = null;

        foreach (var namedParameter in Parameters.OfType<INamedParameterData>()) {
            if (namedParameter.Alias is { } alias) {
                if (aliases.ContainsKey(alias)) {
                    warnedAliases ??= new(2) { aliases[alias] };
                    warnedAliases.Add(namedParameter);
                }
                else {
                    aliases.Add(alias, namedParameter);
                }
            }

            var name = namedParameter.Name;
            if (names.ContainsKey(name)) {
                warnedNames ??= new(2) { names[name] };
                warnedNames.Add(namedParameter);
            }
            else {
                names.Add(name, namedParameter);
            }
        }

        if (warnedAliases is null && warnedNames is null)
            return Filter.Success;

        Diagnostic[] diagnostics = new Diagnostic[warnedAliases?.Count ?? 0 + warnedNames?.Count ?? 0];
        int index = 0;
        foreach (var parameter in warnedAliases.EmptyIfNull()) {
            diagnostics[index++] = DiagnosticFactory.Create(
                DiagnosticDescriptors.NamedParameterAliasRepeat,
                parameter.Model.Syntax);
        }
        foreach (var parameter in warnedNames.EmptyIfNull()) {
            diagnostics[index++] = DiagnosticFactory.Create(
                DiagnosticDescriptors.NamedParameterNameRepeat,
                parameter.Model.Syntax);
        }

        return Filter.CreateDiagnostic(diagnostics);
    }
}
