using Microsoft.CodeAnalysis;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas.Markers;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
internal sealed class OptionParameterData(ParameterModel model) : InputParameterData(model), INamedParameterData, IRequiredParameterData
{
    private Optional<string?> _alias;
    public string? Alias
    {
        get {
            if (!_alias.HasValue) {
                _alias = Model.Attribute?.GetConstructorArgument<string>(Literals.OptionAttribute_Alias_CtorParameterIndex);
            }
            return _alias.Value;
        }
    }

    private string? _name;
    public string Name => _name ??=
        (Model.Attribute?.GetNamedArgument<string>(Literals.OptionAttribute_Name_PropertyIdentifier) ?? Model.Symbol.Name);

    private bool? _isRequired;
    public bool IsRequired
    {
        get {
            _isRequired ??=
                (Model.Attribute?.GetNamedArgument<bool>(Literals.IRequiredParameterAttribute_Required_PropertyIdentifier) ?? false);
            return _isRequired.GetValueOrDefault();
        }
    }
}
