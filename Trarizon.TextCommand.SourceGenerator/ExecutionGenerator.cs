using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Text;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models;
using Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;

namespace Trarizon.TextCommand.SourceGenerator;
[Generator(LanguageNames.CSharp)]
public class ExecutionGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var langVersionResult = context.ParseOptionsProvider.Select((options, token) =>
        {
            if (options is CSharpParseOptions csOption && csOption.LanguageVersion >= LanguageVersion.CSharp12) {
                return null;
            }
            return Diagnostic.Create(
                DiagnosticDescriptors.RequiresLangVersionCSharp12,
                null);
        });

        var filter = context.SyntaxProvider.ForAttributeWithMetadataName(
            Literals.ExecutionAttribute_TypeName,
            (_, _) => true,
            (context, token) =>
            {
                token.ThrowIfCancellationRequested();
                var res = new ExecutionModel(context);
                return (res, res.Validate());
            });

        context.RegisterSourceOutput(langVersionResult, (context, source) =>
        {
            if (source is not null) {
                context.ReportDiagnostic(source);
            }
        });

        context.RegisterSourceOutput(filter, (context, filter) =>
        {
            foreach (var diag in filter.Item2.OfNotNull()) {
                context.ReportDiagnostic(diag);
            }

            var provider = new ExecutionProvider(filter.res).Command;

            var compilation = SyntaxFactory.CompilationUnit(
                default,
                default,
                default,
                SyntaxFactory.List(new[] {
                    provider.PartialTypeDeclaration()
                        .WithLeadingTrivia(
                            SyntaxFactory.Trivia(
                                SyntaxFactory.NullableDirectiveTrivia(
                                    SyntaxFactory.Token(SyntaxKind.EnableKeyword), true)),
                            SyntaxFactory.Trivia(
                                SyntaxFactory.PragmaWarningDirectiveTrivia(
                                    SyntaxFactory.Token(SyntaxKind.DisableKeyword),
                                    SyntaxFactory.SeparatedList<ExpressionSyntax>( new[]{
                                        SyntaxFactory.IdentifierName(Constants.PartialMethodDeclarationHaveSignatureDifferences_ErrorCode),
                                        SyntaxFactory.IdentifierName(Constants.LabelNotBeenReferenced_ErrorCode),
                                        SyntaxFactory.IdentifierName(Constants.PossibleNullReferenceArgumentForParameter_ErrorCode),
                                        SyntaxFactory.IdentifierName(Constants.ReferenceTypeNullableAnnotationNotMatch_ErrorCode),
                                    }),
                                    true))),
                    provider.ParsingContextTypeDeclaration(),
                }));

            context.AddSource(
                provider.GenerateFileName(),
                compilation.NormalizeWhitespace().GetText(Encoding.UTF8));

        });
    }
}