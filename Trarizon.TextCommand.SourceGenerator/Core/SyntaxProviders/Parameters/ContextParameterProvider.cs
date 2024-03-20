using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Trarizon.TextCommand.SourceGenerator.Core.Models;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
using Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.ParameterDatas;
using Trarizon.TextCommand.SourceGenerator.Utilities;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.Parameters;
internal class ContextParameterProvider : IParameterProvider, IParameterDataProvider
{
    public ParameterModel Model { get; }

    public ExecutorProvider Executor { get; }

    public ContextParameterData Data { get; }

    public IParameterDataProvider ParameterData => this;

    public IParameterProvider Parameter => this;

    IParameterData IParameterDataProvider.Data => Data;

    public ContextParameterProvider(ParameterModel model, ExecutorProvider executor)
    {
        Model = model;
        Data = (ContextParameterData)model.ParameterData!;
        Executor = executor;
    }

    public ArgumentSyntax ResultValueArgumentExpression()
    {
        return SyntaxFactory.Argument(
            null,
            SyntaxProvider.RefKindToken(Model.Symbol.RefKind),
            ResultValueAccessExpression());
    }

    public ExpressionSyntax ResultValueAccessExpression() =>SyntaxFactory.IdentifierName(Data.ParameterName);
}
