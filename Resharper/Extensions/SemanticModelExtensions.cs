using Microsoft.CodeAnalysis;

namespace Resharper.Extensions;

public static class SemanticModelExtensions
{
    public static ISymbol? GetSymbol(this SemanticModel semanticModel, SyntaxNode syntaxNode) => semanticModel.GetDeclaredSymbol(syntaxNode) ?? semanticModel.GetSymbolInfo(syntaxNode).Symbol;
}