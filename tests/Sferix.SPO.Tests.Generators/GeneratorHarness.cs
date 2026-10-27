using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Sferix.SPO.Generators;

namespace Sferix.SPO.Tests.Generators;

internal sealed record GeneratorRun(Compilation Output, ImmutableArray<Diagnostic> GeneratorDiagnostics, ImmutableArray<SyntaxTree> GeneratedTrees)
{
    public IEnumerable<string> GeneratorDiagnosticIds => GeneratorDiagnostics.Select(d => d.Id);

    public IEnumerable<Diagnostic> CompilationErrors =>
        Output.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error && !d.Id.StartsWith("SPO", StringComparison.Ordinal));
}

internal static class GeneratorHarness
{
    private static readonly ImmutableArray<MetadataReference> References =
        [.. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))];

    public static CSharpCompilation CreateCompilation(params SyntaxTree[] trees) =>
        CSharpCompilation.Create(
            "GeneratorTestAssembly",
            trees,
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

    public static GeneratorRun Run(params string[] sources)
    {
        var options = new CSharpParseOptions(LanguageVersion.Latest);
        var compilation = CreateCompilation([.. sources.Select(s => CSharpSyntaxTree.ParseText(s, options))]);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SpoMetadataGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);
        return new GeneratorRun(output, diagnostics, driver.GetRunResult().GeneratedTrees);
    }
}
