﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.ParameterDatas;
internal interface INamedParameterDataProvider : IParameterDataProvider
{
    new INamedParameterData Data { get; }

    ExpressionSyntax GetParameterSetDictValue();
}
