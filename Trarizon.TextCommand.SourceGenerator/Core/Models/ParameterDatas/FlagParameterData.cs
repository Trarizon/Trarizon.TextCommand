using Microsoft.CodeAnalysis;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas.Markers;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
internal sealed class FlagParameterData(ParameterModel model) : InputParameterData(model), INamedParameterData, IFlagParameterData
{
    private Optional<string?> _alias;
    public string? Alias
    {
        get {
            if (!_alias.HasValue) {
                _alias = Model.Attribute?.GetConstructorArgument<string>(Literals.FlagAttribute_Alias_CtorParameterIndex);
            }
            return _alias.Value;
        }
    }

    private string? _name;
    public string Name => _name
        ??= (Model.Attribute?.GetNamedArgument<string>(Literals.FlagAttribute_Name_PropertyIdentifier)
            ?? Model.Symbol.Name);
}
