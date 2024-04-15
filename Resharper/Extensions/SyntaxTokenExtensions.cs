using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Resharper.Extensions;

public static class SyntaxTokenExtensions
{
    public static bool IsBeginOfLine(this SyntaxToken syntaxToken) => syntaxToken.GetPreviousToken().IsKind(SyntaxKind.None) || syntaxToken.GetPreviousToken().TrailingTrivia.Any(x => x.IsKind(SyntaxKind.EndOfLineTrivia));
}