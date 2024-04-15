using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Resharper.Extensions;

public static class SolutionExtensions
{
    public static async Task<Solution> RenameAsync(this Solution solution, DocumentId documentId, int position, string name) => await RenameAsync(solution, (await (await (solution.GetDocument(documentId)!).GetSyntaxTreeAsync())!.GetRootAsync()).FindToken(position), name);

    private static async Task<Solution> RenameAsync(this Solution solution, SyntaxToken identifierToken, string name)
    {
        foreach (var x in Gotos.Usages((await solution.GetProject(solution.GetProjectDependencyGraph().GetTopologicallySortedProjects().First())!.GetCompilationAsync())!, identifierToken).GroupBy(x => x.SyntaxTree!))
            solution = solution.WithDocumentSyntaxRoot(solution.GetDocumentId(x.Key)!, (await x.Key.GetRootAsync()).ReplaceTokens(x, (y, _) => SyntaxFactory.Identifier(name).WithTriviaFrom(y)));
        return solution;
    }
}