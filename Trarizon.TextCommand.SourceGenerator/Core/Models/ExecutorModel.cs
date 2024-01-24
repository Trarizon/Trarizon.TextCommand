using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models.Parameters;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;
using Trarizon.TextCommand.SourceGenerator.Utilities.Factories;
using Trarizon.TextCommand.SourceGenerator.Utilities.Toolkit;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models;
internal sealed class ExecutorModel(ExecutionModel execution, MethodDeclarationSyntax syntax, IMethodSymbol symbol, IReadOnlyList<AttributeData> attributes)
{
    public ExecutionModel Execution => execution;

    public MethodDeclarationSyntax Syntax => syntax;
    public IMethodSymbol Symbol => symbol;

    private readonly IReadOnlyList<AttributeData> _attributes = attributes;

    public ParameterModel[]? _parameters;
    public ParameterModel[] Parameters
    {
        get {
            if (_parameters is null) {
                var syntaxes = Syntax.ParameterList.Parameters;
                var symbols = Symbol.Parameters;
                var count = Math.Min(syntaxes.Count, symbols.Length);
                _parameters = new ParameterModel[count];
                for (int i = 0; i < count; i++) {
                    _parameters[i] = new(this, syntaxes[i], symbols[i]);
                }
            }
            return _parameters;
        }
    }

    // Attribute Data

    private string[][]? _commandPrefixes;
    public string[][] CommandPrefixes
    {
        get {
            if (_commandPrefixes is null) {
                _commandPrefixes = new string[_attributes.Count][];
                for (int i = 0; i < _commandPrefixes.Length; i++) {
                    _commandPrefixes[i] = _attributes[i].GetConstructorArguments<string>(Literals.ExecutorAttribute_CommandPrefixes_CtorParameterIndex) ?? [];
                }
            }
            return _commandPrefixes;
        }
    }

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

        if (Execution.Context.SemanticModel.Compilation.ClassifyCommonConversion(
            Symbol.ReturnType,
            Execution.Symbol.ReturnType).Exists
        ) {
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
                if (p.ParameterKind is CLParameterKind.Value or CLParameterKind.MultiValue)
                    errors.Add(DiagnosticFactory.Create(
                        DiagnosticDescriptors.ValueOrMultiValueAfterRestValueWillAlwaysDefault,
                        p.Syntax));
            }
            else if (p.ParameterKind == CLParameterKind.MultiValue && ((MultiValueParameterModel)p.CLParameter).IsRest)
                hasRestValues = true;
        }

        if (errors.Count > 0)
            return Filter.CreateDiagnostic(errors);
        else
            return Filter.Success;
    }

    public Filter ValidateOptionKeys()
    {
        Dictionary<string, ParameterModel> aliases = [];
        Dictionary<string, ParameterModel> names = [];

        HashSet<ParameterModel>? aliasRepeats = null;
        HashSet<ParameterModel>? nameRepeats = null;

        foreach (var parameter in Parameters) {
            if (parameter.CLParameter is INamedParameterModel namedParameter) {
                if (namedParameter.Alias is { } alias) {
                    if (aliases.ContainsKey(alias)) {
                        (aliasRepeats ??= []).Add(aliases[alias]); // No error when repeat add
                        aliasRepeats.Add(parameter);
                    }
                    else {
                        aliases.Add(alias, parameter);
                    }
                }

                var name = namedParameter.Name;
                if (names.ContainsKey(name)) {
                    (nameRepeats ??= []).Add(names[name]);
                    nameRepeats.Add(parameter);
                }
                else {
                    names.Add(name, parameter);
                }
            }
        }

        if (aliasRepeats is null && nameRepeats is null)
            return Filter.Success;

        Diagnostic[] diagnostics = new Diagnostic[aliasRepeats?.Count ?? 0 + nameRepeats?.Count ?? 0];
        int index = 0;
        foreach (var parameter in aliasRepeats.EmptyIfNull()) {
            diagnostics[index++] = DiagnosticFactory.Create(
                DiagnosticDescriptors.NamedParameterAliasRepeat,
                parameter.Syntax);
        }
        foreach (var parameter in nameRepeats.EmptyIfNull()) {
            diagnostics[index++] = DiagnosticFactory.Create(
                DiagnosticDescriptors.NamedParameterNameRepeat,
                parameter.Syntax);
        }

        return Filter.CreateDiagnostic(diagnostics);
    }
}
