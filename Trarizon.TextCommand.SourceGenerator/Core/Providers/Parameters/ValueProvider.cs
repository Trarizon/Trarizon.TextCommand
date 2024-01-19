using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models.Parameters;
using Trarizon.TextCommand.SourceGenerator.Utilities;

namespace Trarizon.TextCommand.SourceGenerator.Core.Providers.Parameters;
internal sealed class ValueProvider(ExecutorProvider executor, ValueParameterModel model, int index) : ParameterProvider(executor)
{
    protected override ICLParameterModel Model => model;

    protected override (string Identifier, ArgumentSyntax[] Arguments) GetProviderMethodInfo()
    {
        return (Literals.ArgsProvider_GetValue_MethodIdentifier, [
            SyntaxFactory.Argument(
                SyntaxProvider.LiteralInt32Expression(index)),
            ParserArgumentSyntax,
            SyntaxFactory.Argument(
                model.Required
                ? SyntaxProvider.LiteralStringExpression($"{Literals.Prefix}{model.Parameter.Symbol.Name}")
                : SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)),
        ]);
    }
}
