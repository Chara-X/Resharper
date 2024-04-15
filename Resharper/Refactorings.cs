using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Resharper;

public static class Refactorings
{
    public static SyntaxTree MoveToAnotherFile(ClassDeclarationSyntax classDeclarationSyntax) => SyntaxFactory.SyntaxTree(SyntaxFactory.FileScopedNamespaceDeclaration(classDeclarationSyntax.SyntaxTree.GetRoot().DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>().Single(x => x.IsKind(SyntaxKind.FileScopedNamespaceDeclaration)).Name).AddMembers(classDeclarationSyntax).NormalizeWhitespace(eol: "\n"));
}