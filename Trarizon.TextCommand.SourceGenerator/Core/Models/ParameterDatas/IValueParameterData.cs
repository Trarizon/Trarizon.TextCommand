﻿namespace Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
internal interface IValueParameterData : IParameterData
{
    int MaxCount { get; }
    int Index { get; set; }
    bool IsRest { get; }
}
