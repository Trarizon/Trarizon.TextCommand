using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models.Parameters;
using Trarizon.TextCommand.SourceGenerator.Utilities;

namespace Trarizon.TextCommand.SourceGenerator.Core.Providers.Parameters;
internal sealed class FlagProvider(ExecutorProvider executor, FlagParameterModel model) : ParameterProvider(executor), INamedParameterProvider
{
    public string? Alias => model.Alias;

    public string Name => model.Name;

    protected override ICLParameterModel Model => model;

    protected override (string Identifier, ArgumentSyntax[] Arguments) GetProviderMethodInfo()
    {
        return (Literals.ArgsProvider_GetFlag_MethodIdentifier, [
            SyntaxFactory.Argument(
                SyntaxProvider.LiteralStringExpression(Literals.FullName(model.Name))),
            ParserArgumentSyntax,
        ]);
    }
}
