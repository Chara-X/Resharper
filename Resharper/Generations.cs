using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Resharper.Extensions;

namespace Resharper;

public static class Generations
{
    public static ClassDeclarationSyntax Constructor(ClassDeclarationSyntax classDeclarationSyntax)
    {
        var fields = classDeclarationSyntax.Members.OfType<FieldDeclarationSyntax>().Select(x => (type: x.Declaration.Type, name: x.Declaration.Variables[0].Identifier.Text)).ToArray();
        return classDeclarationSyntax.AddMembers(SyntaxFactory.ConstructorDeclaration(classDeclarationSyntax.Identifier.Text).AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword)).AddParameterListParameters(fields.Select(x => SyntaxFactory.Parameter(SyntaxFactory.Identifier(x.name[1..])).WithType(x.type)).ToArray()).WithBody(SyntaxFactory.Block(fields.Select(x => SyntaxFactory.ParseStatement($"{x.name} = {x.name[1..]};")))).NormalizeWhitespace(eol: "\n").WithIndention(classDeclarationSyntax.GetIndention() + "    ").WithTrailingTrivia(SyntaxFactory.EndOfLine("\n")));
    }
}