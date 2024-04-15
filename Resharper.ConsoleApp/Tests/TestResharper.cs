using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Text;

namespace Resharper.ConsoleApp.Tests;

public class TestResharper
{
    public static string ProgramText = File.ReadAllText(@"F:\文档\Microsoft Visual Studio\Resharper\Resharper.ConsoleApp\Program.cs");
    public static string ProgramPath = @"F:\文档\Microsoft Visual Studio\Resharper\Resharper.ConsoleApp\Program.cs";
    public static string CarText = File.ReadAllText(@"F:\文档\Microsoft Visual Studio\Resharper\Resharper.ConsoleApp\Car.cs");
    public static string CarPath = @"F:\文档\Microsoft Visual Studio\Resharper\Resharper.ConsoleApp\Car.cs";
    public static void TestDiagnostic()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(CarText);
        var compilation = CSharpCompilation.Create("A", new[] { syntaxTree }, new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) });
        //var diagnostics = compilation.GetDiagnostics();
        //var diagnostics = syntaxTree.GetDiagnostics();
        var diagnostics = syntaxTree.GetRoot().GetDiagnostics();
        foreach (var x in diagnostics)
        {
            Console.WriteLine(x);
        }
    }

    public static async Task TestSymbolFinder()
    {
        using var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("A", LanguageNames.CSharp);
        workspace.AddDocument(project.Id, "A", SourceText.From(CarText));
        foreach (var x in await SymbolFinder.FindDeclarationsAsync(project, "Program", true))
        {
            Console.WriteLine(x.Name);
        }
    }

    public static void TestEmit()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(CarText);
        Console.WriteLine(typeof(object).Assembly.Location);
        Console.WriteLine(typeof(Console).Assembly.Location);
        var dd = typeof(Enumerable).GetTypeInfo().Assembly.Location;
        var coreDir = Directory.GetParent(dd)!;
        Console.WriteLine(coreDir.FullName);
        var cSharpCompilation = CSharpCompilation.Create("A", new[] { syntaxTree }, new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(coreDir.FullName + Path.DirectorySeparatorChar + "System.Runtime.dll")
        });
        var memoryStream = new MemoryStream();
        var emitResult = cSharpCompilation.Emit(memoryStream);
        if (!emitResult.Success)
            foreach (var x in emitResult.Diagnostics)
                Console.WriteLine(x);
        else
        {
            var assembly = Assembly.Load(memoryStream.ToArray());
            var type = assembly.GetTypes().Single(x => x.Name == "Program");
            var textWriter = new StringWriter();
            Console.SetOut(textWriter);
            type.InvokeMember("Main", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, null);
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
            Console.WriteLine("textWriter: " + textWriter);
        }
    }

    public static void TestParseToken()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(CarText);
        var root = syntaxTree.GetRoot();
        // [193,197)
        
        var parseToken = SyntaxFactory.ParseToken(CarText, 192);
        Console.WriteLine(parseToken.Kind() + " : " + parseToken);
    }

    public static void TestAnnotation()
    {
        void ShowCount(SyntaxNode syntaxNode) => Console.WriteLine(syntaxNode.DescendantNodes().Count(x => x.ContainsAnnotations));
        var root = CSharpSyntaxTree.ParseText(CarText).GetRoot();
        ShowCount(root);
        var oldSyntaxNode = root.DescendantNodes().Where(x => x.IsKind(SyntaxKind.MethodDeclaration)).ToArray()[0];
        var newSyntaxNode = oldSyntaxNode.WithAdditionalAnnotations(new SyntaxAnnotation("A", "1"));
        root = root.ReplaceNode(oldSyntaxNode, newSyntaxNode);
        ShowCount(root);
        oldSyntaxNode = root.DescendantNodes().Where(x => x.IsKind(SyntaxKind.MethodDeclaration)).ToArray()[1];
        newSyntaxNode = oldSyntaxNode.WithAdditionalAnnotations(new SyntaxAnnotation("A", "1"));
        root = root.ReplaceNode(oldSyntaxNode, newSyntaxNode);
        ShowCount(root);
        root = root.SyntaxTree.WithChangedText(root.SyntaxTree.GetText().WithChanges(new TextChange(new TextSpan(0,0),"123"))).GetRoot();
        ShowCount(root);
        Console.WriteLine(root);
    }

    public static void TestMoveToAnotherFile(int position)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(ProgramText);
        if (syntaxTree.GetRoot().FindToken(position).Parent is not ClassDeclarationSyntax classDeclarationSyntax) return;
        var newSyntaxTree = Refactorings.MoveToAnotherFile(classDeclarationSyntax);
        File.WriteAllText(ProgramPath, syntaxTree.GetRoot().RemoveNode(classDeclarationSyntax, SyntaxRemoveOptions.KeepNoTrivia)!.ToString());
        File.WriteAllText(Path.Combine(Path.GetDirectoryName(ProgramPath)!, classDeclarationSyntax.Identifier.Text + ".cs"), newSyntaxTree.ToString());
    }

    public static void TestGenerateConstructor(int position)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(ProgramText);
        if (syntaxTree.GetRoot().FindToken(position).Parent is not ClassDeclarationSyntax classDeclarationSyntax) return;
        var newSyntaxTree = syntaxTree.GetRoot().ReplaceNode(classDeclarationSyntax, Generations.Constructor(classDeclarationSyntax));
        File.WriteAllText(ProgramPath, newSyntaxTree.ToString());
    }

    public static async Task TestDocumentEditor()
    {
        using var workspace = new AdhocWorkspace();
        var projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Default, nameof(ConsoleApp), nameof(ConsoleApp), LanguageNames.CSharp);
        workspace.TryApplyChanges(workspace.CurrentSolution.AddProject(projectInfo));
        var document = workspace.AddDocument(projectInfo.Id, "NewFile.cs", SourceText.From(CarText));
        var editor = await DocumentEditor.CreateAsync(document);
        var root = await ((await document.GetSyntaxTreeAsync())!).GetRootAsync();
        var classDeclarationSyntax = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToArray()[0];
        editor.AddMember(classDeclarationSyntax, editor.Generator.FieldDeclaration("name", SyntaxFactory.ParseTypeName("int")));
        Console.WriteLine("Changed:");
        Console.WriteLine("Changed:");
        var newSyntaxTree = (await editor.GetChangedDocument().GetSyntaxTreeAsync())!;
        Console.WriteLine(newSyntaxTree.ToString());
    }

    public static void TestFindNode(int position)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(ProgramText);
        var syntaxNode = syntaxTree.GetRoot().FindNode(new TextSpan(position, 1));
        Console.WriteLine(syntaxNode.Kind());
    }

    public static Task TestWorkSpace()
    {
        const string path = @"F:\文档\Microsoft Visual Studio\Resharper\Resharper.ConsoleApp\Car.cs";

        // 创建 project
        var projectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Default, nameof(ConsoleApp), nameof(ConsoleApp), LanguageNames.CSharp);
        var otherProjectInfo = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Default, nameof(Resharper), nameof(Resharper), LanguageNames.CSharp);

        // 创建document
        var documentInfo = DocumentInfo.Create(DocumentId.CreateNewId(projectInfo.Id), Path.GetFileNameWithoutExtension(path)).WithTextLoader(new FileTextLoader(path, Encoding.UTF8));

        projectInfo = projectInfo.WithParseOptions(new CSharpParseOptions(LanguageVersion.CSharp11)).WithDocuments(new[] { documentInfo }).WithMetadataReferences(new[] { typeof(object).Assembly }.Select(x => x.Location).Distinct().Select(x => MetadataReference.CreateFromFile(x)));
        
        // 创建 Workspace
        using var workspace = new AdhocWorkspace();
        //workspace.Dump(nameof(Workspace.Services), nameof(Workspace.CurrentSolution), nameof(Workspace.Options));
        // 将项目添加到 workspace
        var project = workspace.AddProject(projectInfo);
        workspace.AddProject(otherProjectInfo);
        var document = workspace.CurrentSolution.GetDocument(documentInfo.Id)!;
        document = document.WithName("User");
        Console.WriteLine(string.Join(',', workspace.CurrentSolution.Projects.Select(x => x.GetHashCode())));
        Console.WriteLine(string.Join(',', document.Project.Solution.Projects.Select(x => x.GetHashCode())));
        workspace.TryApplyChanges(document.Project.Solution);
        Console.WriteLine(string.Join(',', workspace.CurrentSolution.Projects.Select(x => x.GetHashCode())));
        Console.WriteLine(string.Join(',', document.Project.Solution.Projects.Select(x => x.GetHashCode())));
        return Task.CompletedTask;

        // 获取编译结果
        //var compilation = (await project.WithCompilationOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication)).GetCompilationAsync())!;
        //foreach (var x in compilation.GetSymbolsWithName(_ => true, SymbolFilter.All))
        //{
        //    Console.WriteLine(x.Name);
        //}
    }
    public static Assembly CompileSourceCode()
    {
        // Create a syntax tree from the source code
        var syntaxTree = CSharpSyntaxTree.ParseText(ProgramText);
        // Create a compilation object that represents the code to be compiled
        var cSharpCompilation = CSharpCompilation.Create("HelloRoslyn.dll",
            new[] { syntaxTree },
            new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        // Emit the compiled code to memory
        using var memoryStream = new MemoryStream();
        var emitResult = cSharpCompilation.Emit(memoryStream);
        if (!emitResult.Success)
        {
            // If the code fails to compile, throw an exception
            throw new Exception("Compilation failed!");
        }

        // Load the compiled code into memory
        memoryStream.Seek(0, SeekOrigin.Begin);
        return Assembly.Load(memoryStream.ToArray());
    }
}