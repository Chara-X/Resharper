using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Resharper.Extensions;

namespace Resharper;

public class Gotos
{
    public static IEnumerable<SyntaxToken> Usages(Compilation compilation, SyntaxToken identifierToken)
    {
        if (identifierToken.Parent == null || !identifierToken.IsKind(SyntaxKind.IdentifierToken) || identifierToken.SyntaxTree == null) throw new ArgumentException(nameof(identifierToken));
        var semanticModel = compilation.GetSemanticModel(identifierToken.SyntaxTree);
        var symbol = semanticModel.GetSymbol(identifierToken.Parent);
        return compilation.SyntaxTrees.Select(x => x.GetRoot().DescendantTokens().Where(y =>
        {
            semanticModel = compilation.GetSemanticModel(y.SyntaxTree!);
            return y.IsKind(SyntaxKind.IdentifierToken) && y.Parent != null && semanticModel.GetSymbol(y.Parent) != null && semanticModel.GetSymbol(y.Parent)!.Equals(symbol);
        }).Prepend(identifierToken)).SelectMany(x => x);
    }
}