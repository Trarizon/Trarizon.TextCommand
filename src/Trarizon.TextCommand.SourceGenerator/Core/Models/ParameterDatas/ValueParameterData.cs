using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas.Markers;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
internal sealed class ValueParameterData(ParameterModel model) : InputParameterData(model), IRequiredParameterData, IPositionalParameterDataMutable
{
    public int StartIndex { get; set; }

    public bool IsUnreachable => StartIndex < 0;

    private bool? _isRequired;
    public bool IsRequired
    {
        get {
            _isRequired ??=
                (Model.Attribute?.GetNamedArgument<bool>(Literals.IRequiredParameterAttribute_Required_PropertyIdentifier) ?? false);
            return _isRequired.GetValueOrDefault();
        }
    }

    int IPositionalParameterData.MaxCount => 1;

    bool IPositionalParameterData.IsRest => false;
}
