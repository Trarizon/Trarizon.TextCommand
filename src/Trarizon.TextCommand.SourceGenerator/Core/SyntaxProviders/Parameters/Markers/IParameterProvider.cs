using Microsoft.CodeAnalysis.CSharp.Syntax;
using Trarizon.TextCommand.SourceGenerator.Core.Models;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas.Markers;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.Parameters.Markers;
internal interface IParameterProvider
{
    IParameterData Data { get; }

    ExecutorProvider Executor { get; }

    ArgumentSyntax ExecutorArgAccess_ArgumentSyntax();
}

internal static class ParameterProvider
{
    public static IParameterProvider? Create(ParameterModel model, ExecutorProvider executor)
    {
        if (!model.IsValid)
            return null;

        return model.Data switch
        {
            IParameterData { IsValid: false } => null,
            FlagParameterData flag => new FlagParameterProvider(flag, executor),
            OptionParameterData opt => new OptionParameterProvider(opt, executor),
            ValueParameterData val => new ValueParameterProvider(val, executor),
            MultiValueParameterData mval => new MultiValueParameterProvider(mval, executor),
            ContextParameterData cp => new ContextParameterProvider(cp, executor),
            _ => throw new System.InvalidOperationException(),
        };
    }
}