using Microsoft.CodeAnalysis.CSharp.Syntax;
using Trarizon.TextCommand.SourceGenerator.Core.Models;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
using Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.ParameterDatas;
using Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.Parameters;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders;
internal interface IParameterProvider
{
    ParameterModel Model { get; }
    ExecutorProvider Executor { get; }
    IParameterDataProvider ParameterData { get; }

    ArgumentSyntax ResultValueArgumentExpression();
}

static class ParameterProvider
{
    public static IParameterProvider Create(ParameterModel model, ExecutorProvider executor)
    {
        return model.ParameterData switch {
            ContextParameterData => new ContextParameterProvider(model, executor),
            null => throw new System.InvalidOperationException(),
            _ => new InputParameterProvider(model, executor),
        };
    }
}