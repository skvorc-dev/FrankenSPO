using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Sferix.SPO.Generators;
using Xunit;

namespace Sferix.SPO.Tests.Generators;

public sealed class IncrementalCachingTests
{
    [Fact]
    public void InvalidEntityIdModelStaysEqualAfterUnrelatedChange()
    {
        var options = new CSharpParseOptions(LanguageVersion.Latest);
        var entity = CSharpSyntaxTree.ParseText(
            "using Sferix.SPO.Abstractions.Metadata;\n[SpoEntity(\"bad\")] public partial class Customer { }", options);
        var compilation = GeneratorHarness.CreateCompilation(entity);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new SpoMetadataGenerator().AsSourceGenerator()],
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));
        driver = driver.RunGenerators(compilation);

        var changed = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("public class Unrelated { }", options));
        driver = driver.RunGenerators(changed);

        var step = driver.GetRunResult().Results[0].TrackedSteps["SpoEntities"];
        var reason = Assert.Single(Assert.Single(step).Outputs).Reason;
        Assert.True(reason is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged, reason.ToString());
    }
}
