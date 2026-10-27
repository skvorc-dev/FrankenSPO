using Xunit;
namespace Sferix.SPO.Tests.Generators;

public sealed class DiagnosticTests
{
    private const string Usings = "using Sferix.SPO.Abstractions.Metadata;\n";
    private const string Id1 = "3f2504e0-4f89-11d3-9a0c-0305e82c3301";
    private const string Id2 = "6ba7b810-9dad-11d1-80b4-00c04fd430c8";

    private static GeneratorRun Run(string code) => GeneratorHarness.Run(Usings + code);

    [Fact]
    public void ClassThatIsNotPartialIsReported()
    {
        var run = Run($"[SpoEntity(\"{Id1}\")] public class Customer {{ [SpoField(1)] public string Name {{ get; set; }} = \"\"; }}");
        Assert.Equal(["SPO0001"], run.GeneratorDiagnosticIds);
        Assert.Empty(run.GeneratedTrees);
    }

    [Fact]
    public void ClassNestedInNonPartialClassIsReported()
    {
        var run = Run($"public class Outer {{ [SpoEntity(\"{Id1}\")] public partial class Customer {{ }} }}");
        Assert.Equal(["SPO0001"], run.GeneratorDiagnosticIds);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public void MissingOrInvalidEntityIdIsReported(string id)
    {
        var run = Run($"[SpoEntity(\"{id}\")] public partial class Customer {{ }}");
        Assert.Equal(["SPO0002"], run.GeneratorDiagnosticIds);
        Assert.Empty(run.GeneratedTrees);
    }

    [Fact]
    public void NullEntityIdIsReported()
    {
        var run = Run("[SpoEntity(null!)] public partial class Customer { }");
        Assert.Equal(["SPO0002"], run.GeneratorDiagnosticIds);
    }

    [Fact]
    public void SameEntityIdTwiceIsReportedOnBothClasses()
    {
        var run = Run($"[SpoEntity(\"{Id1}\")] public partial class A {{ }} [SpoEntity(\"{Id1.ToUpperInvariant()}\")] public partial class B {{ }}");
        Assert.Equal(["SPO0003", "SPO0003"], run.GeneratorDiagnosticIds);
        Assert.Empty(run.GeneratedTrees);
    }

    [Fact]
    public void DifferentEntityIdsAreAccepted()
    {
        var run = Run($"[SpoEntity(\"{Id1}\")] public partial class A {{ }} [SpoEntity(\"{Id2}\")] public partial class B {{ }}");
        Assert.Empty(run.GeneratorDiagnostics);
        Assert.Equal(2, run.GeneratedTrees.Length);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void FieldNumberThatIsNotPositiveIsReported(int number)
    {
        var run = Run($"[SpoEntity(\"{Id1}\")] public partial class C {{ [SpoField({number})] public int X {{ get; set; }} }}");
        Assert.Equal(["SPO0004"], run.GeneratorDiagnosticIds);
        Assert.Empty(run.GeneratedTrees);
    }

    [Fact]
    public void FieldNumberUsedTwiceInOneEntityIsReported()
    {
        var run = Run($"[SpoEntity(\"{Id1}\")] public partial class C {{ [SpoField(1)] public int X {{ get; set; }} [SpoField(1)] public int Y {{ get; set; }} }}");
        var diagnostic = Assert.Single(run.GeneratorDiagnostics);
        Assert.Equal("SPO0004", diagnostic.Id);
        Assert.Contains("'Y'", diagnostic.GetMessage());
    }

    [Fact]
    public void SameFieldNumberInDifferentEntitiesIsAccepted()
    {
        var run = Run($"[SpoEntity(\"{Id1}\")] public partial class A {{ [SpoField(1)] public int X {{ get; set; }} }} [SpoEntity(\"{Id2}\")] public partial class B {{ [SpoField(1)] public int X {{ get; set; }} }}");
        Assert.Empty(run.GeneratorDiagnostics);
    }

    [Theory]
    [InlineData("object")]
    [InlineData("System.Collections.Generic.List<int>")]
    [InlineData("short")]
    [InlineData("int[]")]
    [InlineData("(int, int)")]
    public void UnsupportedPropertyTypeIsReported(string type)
    {
        var run = Run($"[SpoEntity(\"{Id1}\")] public partial class C {{ [SpoField(1)] public {type} X {{ get; set; }} = default!; }}");
        Assert.Equal(["SPO0005"], run.GeneratorDiagnosticIds);
        Assert.Empty(run.GeneratedTrees);
    }

    [Theory]
    [InlineData("public int X { get; }")]
    [InlineData("public int X { set { } }")]
    public void FieldPropertyWithoutGetterAndSetterIsReported(string property)
    {
        var run = Run($"[SpoEntity(\"{Id1}\")] public partial class C {{ [SpoField(1)] {property} }}");
        Assert.Equal(["SPO0006"], run.GeneratorDiagnosticIds);
        Assert.Empty(run.GeneratedTrees);
    }

    [Fact]
    public void PropertyWithoutFieldAttributeIsIgnored()
    {
        var run = Run($"[SpoEntity(\"{Id1}\")] public partial class C {{ public object Cache {{ get; }} = new(); }}");
        Assert.Empty(run.GeneratorDiagnostics);
    }
}
