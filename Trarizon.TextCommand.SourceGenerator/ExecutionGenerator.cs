using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Text;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models;
using Trarizon.TextCommand.SourceGenerator.Core.Providers;
using Trarizon.TextCommand.SourceGenerator.Utilities.Toolkit;
using Trarizon.TextCommand.SourceGenerator.Utilities.Toolkit.Extensions;

namespace Trarizon.TextCommand.SourceGenerator;
[Generator(LanguageNames.CSharp)]
public class ExecutionGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var langVersionResult = context.ParseOptionsProvider.Select((options, token) =>
        {
            if (options is CSharpParseOptions csOption && csOption.LanguageVersion >= LanguageVersion.CSharp12) {
                return Filter.Success;
            }
            return Filter.CreateDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.RequiresLangVersionCSharp12,
                null));
        });

        var filter = context.SyntaxProvider.ForAttributeWithMetadataName(
            Literals.ExecutionAttribute_TypeName,
            static (node, _) => true,
            static (context, token) =>
            {
                token.ThrowIfCancellationRequested();

                return Filter.Select(context, static context =>
                {
                    var methodSyntax = (MethodDeclarationSyntax)context.TargetNode;
                    if (methodSyntax.GetParent<TypeDeclarationSyntax>() is { } typeSyntax) {
                        return Filter.Create(new ContextModel(context, typeSyntax).ExecutionModel);
                    }
                    else {
                        // Cannot be global, compiler will warn this
                        return default;
                    }
                })
                .ThrowIfCancelled(token)
                .Predicate(e => e.ValidatePartialKeyWord())
                .Predicate(e => e.ValidateParameter_SetInputParameterType())
                .Predicate(e => e.ValidateReturnType())
                .Predicate(e => e.ValidateCommandName())
                .Predicate(e => e.ValidateExecutorsCommandPrefixes())
                .PredicateMany(e => e.Executors,
                    e => Filter.Create(e)
                    .Predicate(e => e.ValidateStaticKeyword())
                    .Predicate(e => e.ValidateReturnType())
                    .PredicateMany(e => e.Parameters,
                        p => Filter.Create(p)
                        .Predicate(p => p.ValidateSingleAttribute_SetParameterKind())
                        .Predicate(p => p.ValidateCLParameter_SetCLParameter())
                        .CloseIfHasError()
                        .Predicate(p => p.ValidateRequiredParameterNullableAnnotation()))
                    .Predicate(e => e.ValidateValueParametersAfterRestValues())
                    .Predicate(e => e.ValidateOptionKeys()))
                .CloseIfHasError();
            });

        context.RegisterFilteredSourceOutput(filter, (context, model) =>
        {
            var provider = new ExecutionProvider(model);

            var compilationUnit = SyntaxFactory.CompilationUnit(
                default,
                default,
                default,
                SyntaxFactory.List(new[] {
                    provider.Command.ClonePartialTypeDeclaration()
                        .WithLeadingTrivia(
                            SyntaxFactory.TriviaList(
                                SyntaxFactory.Trivia(
                                    SyntaxFactory.NullableDirectiveTrivia(
                                        SyntaxFactory.Token(SyntaxKind.EnableKeyword), true)),
                                SyntaxFactory.Trivia(
                                    SyntaxFactory.PragmaWarningDirectiveTrivia(
                                        SyntaxFactory.Token(SyntaxKind.DisableKeyword),
                                        SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                            SyntaxFactory.IdentifierName(Constants.PartialMethodDeclarationHaveSignatureDifferences_ErrorCode)),
                                        true)))),
                    provider.ParameterSets_ClassDeclaration(),
                }));

            context.AddSource(
                $"{provider.Command.Symbol.ToDisplayString().Replace('<', '{').Replace('>', '}')}.TextCommand.g.cs",
                compilationUnit.NormalizeWhitespace().GetText(Encoding.UTF8));
        });
    }
}
