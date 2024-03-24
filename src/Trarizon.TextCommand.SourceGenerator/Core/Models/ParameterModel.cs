using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas.Markers;
using Trarizon.TextCommand.SourceGenerator.Core.Tags;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;
using Trarizon.TextCommand.SourceGenerator.Utilities.Factories;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models;
internal sealed class ParameterModel(ExecutorModel executor)
{
    [MemberNotNullWhen(true, nameof(Data))]
    public bool IsValid => Data is not null;

    public SemanticModel SemanticModel => Executor.SemanticModel;

    public ExecutorModel Executor { get; } = executor;

    public IParameterData? Data { get; private set; } = default!;

    /// <summary>
    /// Defination parameter
    /// </summary>
    public required IParameterSymbol Symbol { get; init; }

    /// <summary>
    /// Defination parameter
    /// </summary>
    private ParameterSyntax? _syntax;
    public ParameterSyntax Syntax => _syntax ??= (ParameterSyntax)Symbol.DeclaringSyntaxReferences[0].GetSyntax();

    // Set in ValidateAttribute
    public AttributeData? Attribute { get; private set; }

    // Data

    /// <summary>
    /// Kind directly declared in source code
    /// </summary>
    public ExecutorParameterKind DeclaredParameterKind { get; private set; }

    public IEnumerable<Diagnostic?> Validate()
    {
        yield return ValidateAttribute();
        yield return ValidateParameterData();
        if (IsValid) {
            foreach (var d in Data.Validate())
                yield return d;
            yield return ValidateNotRequiredParameterNullableAnnotation();
        }
    }

    private Diagnostic? ValidateAttribute()
    {
        bool isSingleAttr = Symbol.GetAttributes()
            .Select(attr => (attr, ValidationHelper.ValidateExecutorParameter(attr)))
            .TrySingleOrNone(attr => attr.Item2 is not ExecutorParameterKind.Invalid, out var res);
        if (!isSingleAttr) {
            return DiagnosticFactory.Create(
                DiagnosticDescriptors.MarkSingleParameterAttributes,
                Syntax);
        }

        (Attribute, DeclaredParameterKind) = res;
        return null;
    }

    private Diagnostic? ValidateParameterData()
    {
        Debug.Assert(Attribute is null || DeclaredParameterKind is not ExecutorParameterKind.Invalid);

        switch (DeclaredParameterKind) {
            case ExecutorParameterKind.Implicit:
                // TODO: this validation is repeated in Data.SetImplicitParser()
                var ipk = ValidationHelper.ValidateExecutorImplicitParameterKind(Symbol.Type, false, out _);
                Data = ipk switch {
                    ImplicitExecutorParameterKind.Boolean => new FlagParameterData(this),
                    ImplicitExecutorParameterKind.ISpanParsable or
                    ImplicitExecutorParameterKind.Enum => new OptionParameterData(this),
                    _ => null!,
                };
                if (Data is null) {
                    return DiagnosticFactory.Create(
                        DiagnosticDescriptors.ParameterNoImplicitParser,
                        Syntax);
                }
                break;
            case ExecutorParameterKind.Flag:
                Data = new FlagParameterData(this);
                break;
            case ExecutorParameterKind.Option:
                Data = new OptionParameterData(this);
                break;
            case ExecutorParameterKind.Value:
                Data = new ValueParameterData(this);
                break;
            case ExecutorParameterKind.MultiValue:
                Data = new MultiValueParameterData(this);
                break;
            case ExecutorParameterKind.Context:
                Data = new ContextParameterData(this);
                break;
            default:
                throw new InvalidOperationException();
        }
        return null;
    }

    private Diagnostic? ValidateNotRequiredParameterNullableAnnotation()
    {
        Debug.Assert(Data is not null);

        if (Data is IRequiredParameterData { IsRequired: false } and not IMultipleParameterData && !Symbol.Type.IsMayBeDefault()) {
            return DiagnosticFactory.Create(
                    DiagnosticDescriptors.NotRequiredParameterMayBeDefault,
                    Syntax);
        }

        return null;
    }
}
