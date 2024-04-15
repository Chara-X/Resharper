using Microsoft.CodeAnalysis;

namespace Resharper.Extensions;

public static class SymbolExtensions
{
    public static bool IsKinds(this ISymbol symbol, params SymbolKind[] symbolKinds) => symbolKinds.Any(x => x == symbol.Kind);
    public static IEnumerable<INamedTypeSymbol> GetNamedTypeSymbols(this IAssemblySymbol assemblySymbol) => assemblySymbol.Modules.Single().ReferencedAssemblySymbols.Aggregate(assemblySymbol.GlobalNamespace.GetNamedTypeSymbols(), (current, x) => current.Concat(GetNamedTypeSymbols(x)));
    public static IEnumerable<INamedTypeSymbol> GetNamedTypeSymbols(this INamespaceSymbol namespaceSymbol) => namespaceSymbol.GetNamespaceMembers().Aggregate(namespaceSymbol.GetTypeMembers().Where(x => x.DeclaredAccessibility == Accessibility.Public).AsEnumerable(), (current, x) => current.Concat(GetNamedTypeSymbols(x)));
    public static IEnumerable<IMethodSymbol> GetMethodSymbols(INamedTypeSymbol namedTypeSymbol) => namedTypeSymbol.GetMembers().OfType<IMethodSymbol>().Concat(namedTypeSymbol.BaseType != null ? GetMethodSymbols(namedTypeSymbol.BaseType) : Array.Empty<IMethodSymbol>());
}