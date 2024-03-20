using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Text;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models;
using Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders;
using Trarizon.TextCommand.SourceGenerator.Utilities.Toolkit;

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
            static (node, _) => true,
            static (context, token) =>
            {
                token.ThrowIfCancellationRequested();

                var res = new DiagnosticContext<CommandModel>(new CommandModel(context));
                var executor = res
                     .Select(c => c.ExecutionModel)
                     .Validate(e => e.ValidateParameter())
                     .Validate(e => e.ValidateReturnType())
                     .Validate(e => e.ValidateCommandName())
                     .Validate(e => e.ValidateErrorHandler())
                     .Validate(e => e.ValidateExecutorsCommandPrefixes())
                     .SelectMany(e => e.Executors)
                     .Validate(e => e.ValidateStaticKeyword())
                     .Validate(e => e.ValidateReturnType())
                     .Validate(e => e.ValidateCommandPrefixes());
                executor
                     .SelectMany(e => e.Parameters)
                     .Validate(p => p.ValidateSingleAttribute())
                     .Validate(p => p.ValidateParameterData())
                     .Validate(p => p.ValidateRequiredParameterNullableAnnotation());
                executor
                    .Validate(e => e.ValidateOptionKeys())
                    .Validate(e => e.ValidateValueParametersCount());

                return res;
            });

        context.RegisterSourceOutput(langVersionResult, (context, source) =>
        {
            if (source is not null) {
                context.ReportDiagnostic(source);
            }
        });

        context.RegisterSourceOutput(filter, (context, diagContext) =>
        {
            foreach (var diag in diagContext.Diagnostics) {
                context.ReportDiagnostic(diag);
            }

            foreach (var cmd in diagContext.Values) {
                var provider = new CommandProvider(cmd);
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
                        provider.ParameterSetsTypeDeclaration(),
                    }));

                context.AddSource(
                    provider.GeneratedFileName(),
                    compilation.NormalizeWhitespace().GetText(Encoding.UTF8));
            }
        });
    }
}