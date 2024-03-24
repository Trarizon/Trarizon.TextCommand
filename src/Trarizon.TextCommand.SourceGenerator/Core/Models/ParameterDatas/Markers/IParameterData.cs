using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas.Markers;
internal interface IParameterData
{
    bool IsValid { get; }

    ParameterModel Model { get; }

    IEnumerable<Diagnostic?> Validate();
}
