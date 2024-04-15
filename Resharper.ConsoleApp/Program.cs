using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Options;
using Resharper.ConsoleApp.Tests;
using Resharper.Extensions;

// ReSharper disable ConvertToLocalFunction

namespace Resharper.ConsoleApp;
public class User
{
    private int _id;
    private string _name;
    public User(int id, string name)
    {
        _id = id;
        _name = name;
    } }

internal class Program
{
    private static void Main()
    {
        //TestResharper.TestAnnotation();
        var syntaxTree = CSharpSyntaxTree.ParseText(TestResharper.CarText);
        Compilation compilation = CSharpCompilation.Create("A", new[] { syntaxTree }, new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location), MetadataReference.CreateFromFile(typeof(ISymbol).Assembly.Location) });
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var tokens = syntaxTree.GetRoot().DescendantTokens().ToArray();
        Console.WriteLine(tokens.Length);
        foreach (var x in tokens)
        {
            if (x.Parent == null) continue;
            var symbol = semanticModel.GetSymbol(x.Parent);
            if (symbol == null) continue;
            if (symbol.Kind != SymbolKind.NamedType && symbol is not IMethodSymbol { MethodKind: MethodKind.Ordinary }) continue;
            Console.WriteLine(x.Text + " : " + x.Parent.Kind() + " : " + symbol.Name);
        }
    }


    private static void Browse()
    {
        SyntaxTree syntaxTree;
        CSharpSyntaxTree cSharpSyntaxTree;
        ParseOptions parseOptions;
        CSharpParseOptions cSharpParseOptions;

        SyntaxNode syntaxNode;

        SyntaxToken syntaxToken;
        SyntaxTrivia syntaxTrivia;
        SyntaxFactory.IdentifierName("");
        CSharpSyntaxRewriter cSharpSyntaxReWriter;

        Compilation compilation;
        CSharpCompilation cSharpCompilation;
        CSharpCompilationOptions cSharpCompilationOptions;

        SemanticModel semanticModel;

        ISymbol symbol;

        Workspace workspace;
        AdhocWorkspace adhocWorkspace;
        OptionSet optionSet;
        HostServices hostServices;
        HostWorkspaceServices hostWorkspaceServices;

        Solution solution;
        Project project;
        Document document;

        SolutionInfo solutionInfo;
        ProjectInfo projectInfo;
        DocumentInfo documentInfo;
    }
}