using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas.Markers;
using Trarizon.TextCommand.SourceGenerator.Core.Tags;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;
using Trarizon.TextCommand.SourceGenerator.Utilities.Factories;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
internal abstract class InputParameterData(ParameterModel model) : IInputParameterData
{
    public virtual bool IsValid => ParserInfo.Kind is not CustomParserKind.Invalid;

    public ParameterModel Model { get; } = model;

    /// <summary>
    /// May be invalid
    /// </summary>
    public CustomParserInfo ParserInfo { get; protected set; }

    public virtual ITypeSymbol TargetElementTypeSymbol => Model.Symbol.Type;

    public virtual IEnumerable<Diagnostic?> Validate()
    {
        // Implicit
        if (Model.Attribute is null) {
            yield return TrySetImplicitParser();
            yield break;
        }

        var memberParserName = Model.Attribute.GetNamedArgument<string>(Literals.ParameterAttribute_Parser_PropertyIdentifier);
        var typeParserType = Model.Attribute.GetNamedArgument<ITypeSymbol>(Literals.ParameterAttribute_ParserType_PropertyIdentifier);

        switch (memberParserName, typeParserType) {
            case (null, null): {
                yield return TrySetImplicitParser();
                yield break;
            }
            case (not null, not null): {
                yield return DiagnosticFactory.Create(
                    DiagnosticDescriptors.DoNotSpecifyBothParserAndParserType,
                    Model.Syntax);
                yield break;
            }
            case (not null, null): {
                bool found = Model.Executor.Execution.Command.Symbol.GetMembers(memberParserName)
                    .Any(member =>
                    {
                        if (CustomParserInfo.TryMember(member, Model.SemanticModel, TargetElementTypeSymbol, this is IFlagParameterData, out var parserInfo)) {
                            ParserInfo = parserInfo;
                            return true;
                        }
                        return false;
                    });
                if (!found) {
                    yield return DiagnosticFactory.Create(
                        DiagnosticDescriptors.CannotFindValidMemberParser_0MemberName,
                        Model.Syntax,
                        memberParserName);
                }
                yield break;
            }
            case (null, not null): {
                if (CustomParserInfo.TryStruct(typeParserType, Model.SemanticModel, TargetElementTypeSymbol, this is IFlagParameterData, out var parserInfo)) {
                    ParserInfo = parserInfo;
                    yield break;
                }
                else {
                    yield return DiagnosticFactory.Create(
                        DiagnosticDescriptors.CustomTypeParserInvalid,
                        Model.Syntax);
                }
                yield break;
            }
        }
    }

    private Diagnostic? TrySetImplicitParser()
    {
        if (CustomParserInfo.TryImplicit(TargetElementTypeSymbol, this is IFlagParameterData, out var parserInfo)) {
            ParserInfo = parserInfo;
            return null;
        }
        else {
            return DiagnosticFactory.Create(
                DiagnosticDescriptors.ParameterNoImplicitParser,
                Model.Syntax);
        }
    }
}
