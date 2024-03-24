using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas.Markers;
using Trarizon.TextCommand.SourceGenerator.Core.Tags;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;
using Trarizon.TextCommand.SourceGenerator.Utilities.Factories;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
internal sealed class MultiValueParameterData(ParameterModel model) : InputParameterData(model), IMultipleParameterData, IRequiredParameterData, IPositionalParameterDataMutable
{
    public override bool IsValid => CollectionKind is not MultiParameterCollectionKind.Invalid && base.IsValid;

    private ITypeSymbol _parserTargetTypeSymbol = default!;
    public override ITypeSymbol TargetElementTypeSymbol => _parserTargetTypeSymbol;

    public MultiParameterCollectionKind CollectionKind { get; private set; }

    private bool? _isRequired;
    public bool IsRequired
    {
        get {
            _isRequired ??=
                (Model.Attribute?.GetNamedArgument<bool>(Literals.IRequiredParameterAttribute_Required_PropertyIdentifier) ?? false);
            return _isRequired.GetValueOrDefault();
        }
    }

    private int? _maxCount;
    public int MaxCount
    {
        get {
            if (!_maxCount.HasValue) {
                _maxCount ??=
                    (Model.Attribute?.GetConstructorArgument<int>(Literals.MultiValueAttribute_MaxCount_CtorParameterIndex)
                    ?? 0); // If not set, to the rest
            }
            var maxCount = _maxCount.GetValueOrDefault();
            return maxCount > 0 ? maxCount : int.MaxValue - StartIndex;
        }
    }

    public int StartIndex { get; set; }

    public bool IsRest => _maxCount <= 0;

    public bool IsUnreachable => StartIndex < 0;

    public override IEnumerable<Diagnostic?> Validate()
    {
        CollectionKind = ValidationHelper.ValidateMultiParameterCollectionKind(Model.Symbol.Type, out var elementType);
        if (CollectionKind is MultiParameterCollectionKind.Invalid) {
            return DiagnosticFactory.Create(
                 DiagnosticDescriptors.InvalidMultiValueCollectionType,
                 Model.Syntax).Collect();
        }

        _parserTargetTypeSymbol = elementType;

        return base.Validate();
    }
}
