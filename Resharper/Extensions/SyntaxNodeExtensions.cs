using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Resharper.Extensions;

public static class SyntaxNodeExtensions
{
    public static bool IsKinds(this SyntaxNode syntaxNode, params SyntaxKind[] syntaxKinds) => syntaxKinds.Any(syntaxNode.IsKind);
    public static bool HasOpenAndCloseBraceToken(this SyntaxNode syntaxNode) => syntaxNode.ChildTokens().SingleOrDefault(y => y.IsKind(SyntaxKind.OpenBraceToken)) != default && syntaxNode.ChildTokens().SingleOrDefault(y => y.IsKind(SyntaxKind.CloseBraceToken)) != default;
    public static T WithIndention<T>(this T syntaxNode, string indention) where T : SyntaxNode => syntaxNode.ReplaceTokens(syntaxNode.DescendantTokens().Where(x => x.IsBeginOfLine()), (x, _) => x.WithLeadingTrivia(x.LeadingTrivia.Add(SyntaxFactory.Whitespace(indention))));
    public static string GetIndention(this SyntaxNode syntaxNode) => syntaxNode.GetLeadingTrivia().Reverse().TakeWhile(y => y.IsKind(SyntaxKind.WhitespaceTrivia)).Aggregate(string.Empty, (y, z) => y + z);

    public static bool TryFindNode(this SyntaxNode syntaxNode, TextSpan textSpan, SpanType spanType, out SyntaxNode result)
    {
        try
        {
            result = spanType switch
            {
                SpanType.FullSpan => syntaxNode.FindNode(textSpan),
                SpanType.Span => syntaxNode.DescendantNodesAndSelf().Where(x => x.Span.Contains(textSpan)).MinBy(x => x.Span.Length),
                SpanType.BraceSpan => syntaxNode.DescendantNodesAndSelf().Where(x => x.ChildTokens().SingleOrDefault(y => y.IsKind(SyntaxKind.OpenBraceToken)).SpanStart < textSpan.Start && x.ChildTokens().SingleOrDefault(y => y.IsKind(SyntaxKind.CloseBraceToken)).SpanStart > textSpan.Start).MinBy(x => x.Span.Length),
                _ => throw new ArgumentOutOfRangeException(nameof(spanType), spanType, null)
            } ?? throw new NullReferenceException();
            return true;
        }
        catch
        {
            result = default!;
            return false;
        }
    }

    public static bool TryFindToken(this SyntaxNode syntaxNode, TextSpan textSpan, SpanType spanType, out SyntaxToken result)
    {
        try
        {
            result = spanType switch
            {
                SpanType.FullSpan => syntaxNode.FindToken(textSpan.Start),
                SpanType.Span => syntaxNode.DescendantTokens().Where(x => x.Span.Contains(textSpan)).MinBy(x => x.Span.Length),
                SpanType.BraceSpan => throw new ArgumentOutOfRangeException(nameof(spanType), spanType, null),
                _ => throw new ArgumentOutOfRangeException(nameof(spanType), spanType, null)
            };
            return true;
        }
        catch
        {
            result = default!;
            return false;
        }
    }

    public static bool TryFindNode(this SyntaxNode syntaxNode, int position, SpanType spanType, out SyntaxNode result) => TryFindNode(syntaxNode, new TextSpan(position, 1), spanType, out result);
    public static bool TryFindToken(this SyntaxNode syntaxNode, int position, SpanType spanType, out SyntaxToken result) => TryFindToken(syntaxNode, new TextSpan(position, 1), spanType, out result);
    public static SyntaxNode? FindNode(this SyntaxNode syntaxNode, TextSpan textSpan, SpanType spanType) => TryFindNode(syntaxNode, textSpan, spanType, out var result) ? result : null;
    public static SyntaxToken FindToken(this SyntaxNode syntaxNode, TextSpan textSpan, SpanType spanType) => TryFindToken(syntaxNode, textSpan, spanType, out var result) ? result : default;
    public static SyntaxNode? FindNode(this SyntaxNode syntaxNode, int position, SpanType spanType) => TryFindNode(syntaxNode, position, spanType, out var result) ? result : null;
    public static SyntaxToken FindToken(this SyntaxNode syntaxNode, int position, SpanType spanType) => TryFindToken(syntaxNode, position, spanType, out var result) ? result : default;
}

public enum SpanType { FullSpan, Span, BraceSpan }