using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

namespace BitMono.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(IterateMembersNotModuleCodeFixProvider)), Shared]
public sealed class IterateMembersNotModuleCodeFixProvider : CodeFixProvider
{
    private const string Title = "Iterate Context.Parameters.Members.OfType<TypeDefinition>()";

    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(IterateMembersNotModuleAnalyzer.DiagnosticId);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var invocation = root?.FindNode(context.Diagnostics[0].Location.SourceSpan)
            .FirstAncestorOrSelf<InvocationExpressionSyntax>();
        if (invocation is null)
        {
            return;
        }
        context.RegisterCodeFix(
            CodeAction.Create(Title, ct => ReplaceAsync(context.Document, invocation, ct), equivalenceKey: Title),
            context.Diagnostics[0]);
    }

    private static async Task<Document> ReplaceAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        // Simplifier.Annotation collapses the global:: name to just TypeDefinition when the using is present.
        var replacement = SyntaxFactory
            .ParseExpression("Context.Parameters.Members.OfType<global::AsmResolver.DotNet.TypeDefinition>()")
            .WithTriviaFrom(invocation)
            .WithAdditionalAnnotations(Simplifier.Annotation, Formatter.Annotation);
        return document.WithSyntaxRoot(root!.ReplaceNode(invocation, replacement));
    }
}
