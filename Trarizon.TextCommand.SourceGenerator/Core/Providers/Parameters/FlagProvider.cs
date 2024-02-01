using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models;
using Trarizon.TextCommand.SourceGenerator.Core.Models.Parameters;
using Trarizon.TextCommand.SourceGenerator.Utilities;

namespace Trarizon.TextCommand.SourceGenerator.Core.Providers.Parameters;
internal sealed class FlagProvider(ExecutorProvider executor, FlagParameterData data) : ParameterProvider(executor), INamedParameterProvider
{
    public string? Alias => data.Alias;

    public string Name => data.Name;

    protected override ParameterModel Model => data.Model;

    protected override (string Identifier, ArgumentSyntax[] Arguments) GetProviderMethodInfo()
    {
        return (Literals.ArgsProvider_GetFlag_MethodIdentifier, [
            SyntaxFactory.Argument(
                SyntaxProvider.LiteralStringExpression(data.Name)),
            ParserArgumentSyntax,
        ]);
    }
}
