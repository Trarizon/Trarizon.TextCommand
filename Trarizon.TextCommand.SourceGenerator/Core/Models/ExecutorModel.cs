using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;
using Trarizon.TextCommand.SourceGenerator.Utilities.Factories;
using Trarizon.TextCommand.SourceGenerator.Utilities.Toolkit;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models;
internal sealed class ExecutorModel(ExecutionModel execution, MethodDeclarationSyntax syntax, IMethodSymbol symbol, AttributeData attribute)
{
    public ExecutionModel Execution => execution;

    public MethodDeclarationSyntax Syntax => syntax;
    public IMethodSymbol Symbol => symbol;

    private readonly AttributeData _attribute = attribute;

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

    private string[]? _commandPrefixes;
    public string[] CommandPrefixes => _commandPrefixes ??= (_attribute.GetConstructorArguments<string>(Literals.ExecutorAttribute_CommandPrefixes_CtorParameterIndex) ?? []);

    public Filter ValidateReturnType()
    {
        if (Execution.Context.SemanticModel.Compilation.ClassifyCommonConversion(
            Symbol.ReturnType,
            Execution.Symbol.ReturnType).Exists
        ) {
            return Filter.Success;
        }

        return Filter.Create(DiagnosticFactory.Create(
            DiagnosticDescriptors.ExecutorsReturnTypeShouldAssignableToExecutionsReturnType,
            Syntax.ReturnType));
    }
}
