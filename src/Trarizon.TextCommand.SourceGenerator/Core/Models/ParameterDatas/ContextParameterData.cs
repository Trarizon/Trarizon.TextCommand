using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas.Markers;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;
using Trarizon.TextCommand.SourceGenerator.Utilities.Factories;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
internal sealed class ContextParameterData(ParameterModel model) : IParameterData
{
    public bool IsValid => true;

    public ParameterModel Model { get; } = model;

    private string? _parameterName;
    public string ParameterName => _parameterName ??=
        (Model.Attribute?.GetNamedArgument<string>(Literals.ContextParameterAttribute_ParameterName_PropertyIdentifier) ?? Model.Symbol.Name);

    public IEnumerable<Diagnostic?> Validate()
    {
        // Cannot find parameter in execution
        if(!Model.Executor.Execution.Symbol.Parameters.TryFirst(p=>p.Name==ParameterName,out var executionParameter)) {
            yield return DiagnosticFactory.Create(
                DiagnosticDescriptors.ExecutorContextParameterNotFound_0ParameterName,
                Model.Syntax,
                ParameterName);
            yield break;
        }

        // Type not assignable
        if (!Model.SemanticModel.Compilation.ClassifyCommonConversion(executionParameter.Type, Model.Symbol.Type).IsIdentity) {
            yield return DiagnosticFactory.Create(
                DiagnosticDescriptors.CannotPassContextParameterForTypeDifference_0ExecutionParamType,
                Model.Syntax,
                executionParameter.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
        }
        // Ref kind not assignable
        if (!executionParameter.IsRefKindCompatiblyPassTo(Model.Symbol)) {
            yield return DiagnosticFactory.Create(
                DiagnosticDescriptors.CannotPassContextParameterForRefKind,
                Model.Syntax);
        }
        yield break;
    }
}
