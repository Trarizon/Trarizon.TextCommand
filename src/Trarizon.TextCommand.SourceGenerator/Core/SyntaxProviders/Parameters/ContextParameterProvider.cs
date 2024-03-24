using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas.Markers;
using Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.Parameters.Markers;
using Trarizon.TextCommand.SourceGenerator.Utilities;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.Parameters;
internal sealed class ContextParameterProvider(ContextParameterData data, ExecutorProvider executor) : IParameterProvider
{
    public ContextParameterData Data { get; } = data;

    public ExecutorProvider Executor { get; } = executor;

    IParameterData IParameterProvider.Data => Data;

    public ArgumentSyntax ExecutorArgAccess_ArgumentSyntax()
        => SyntaxFactory.Argument(
            null,
            SyntaxProvider.RefKindToken(Data.Model.Symbol.RefKind),
            SyntaxFactory.IdentifierName(Data.ParameterName));
}
