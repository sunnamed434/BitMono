using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace BitMono.Analyzers;

/// <summary>
/// Flags <c>Context.Module.GetAllTypes()</c> inside a protection: it walks the whole module and
/// bypasses the <c>[DoNotResolve]</c> filtering BitMono applies to <c>Context.Parameters.Members</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class IterateMembersNotModuleAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "BITM0001";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Iterate Context.Parameters.Members, not the module",
        messageFormat: "'{0}' walks the whole module and bypasses BitMono's [DoNotResolve] filtering; iterate Context.Parameters.Members.OfType<…>() instead so excluded members are respected",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Inside a protection, Context.Module.GetAllTypes() returns every type and skips the reflection / special-runtime / model / BAML filtering BitMono applies to Context.Parameters.Members. Iterate the sorted member list instead. Suppress this if you genuinely need the raw module (e.g. collecting references to re-sync after renaming).",
        helpLinkUri: "https://bitmono.readthedocs.io/en/latest/developers/do-not-resolve-members.html");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static start =>
        {
            // Only meaningful in a BitMono protection-authoring compilation. If IProtection isn't
            // referenced, this isn't one - do nothing.
            var protectionInterface = start.Compilation.GetTypeByMetadataName("BitMono.API.Protections.IProtection");
            if (protectionInterface is null)
            {
                return;
            }
            start.RegisterSyntaxNodeAction(ctx => Analyze(ctx, protectionInterface), SyntaxKind.InvocationExpression);
        });
    }

    private static void Analyze(SyntaxNodeAnalysisContext context, INamedTypeSymbol protectionInterface)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }
        if (memberAccess.Name.Identifier.ValueText != "GetAllTypes")
        {
            return;
        }
        // The receiver must be an AsmResolver ModuleDefinition (e.g. Context.Module), so we don't trip
        // on some unrelated GetAllTypes() the author happens to define.
        var receiverType = context.SemanticModel.GetTypeInfo(memberAccess.Expression, context.CancellationToken).Type;
        if (receiverType is not { Name: "ModuleDefinition" } ||
            receiverType.ContainingNamespace?.ToDisplayString() != "AsmResolver.DotNet")
        {
            return;
        }
        // Only inside a protection. Anchored on IProtection so it covers built-in protections and
        // external plugins alike, whatever namespace they live in.
        var enclosingType = context.SemanticModel
            .GetEnclosingSymbol(invocation.SpanStart, context.CancellationToken)?.ContainingType;
        if (enclosingType is null || !ImplementsProtection(enclosingType, protectionInterface))
        {
            return;
        }
        context.ReportDiagnostic(Diagnostic.Create(Rule, memberAccess.GetLocation(), memberAccess.ToString()));
    }

    private static bool ImplementsProtection(INamedTypeSymbol type, INamedTypeSymbol protectionInterface)
    {
        foreach (var iface in type.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(iface, protectionInterface))
            {
                return true;
            }
        }
        return false;
    }
}
