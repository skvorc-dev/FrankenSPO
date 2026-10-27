using Xunit;
using System.Reflection;
using Sferix.SPO.Abstractions.Metadata;

namespace Sferix.SPO.Tests.Generators;

public sealed class MetadataGenerationTests
{
    private const string EntityId = "3f2504e0-4f89-11d3-9a0c-0305e82c3301";

    private const string CustomerSource = $$"""
        using Sferix.SPO.Abstractions.Metadata;
        namespace Shop.Model;
        public enum Tier { Basic = 1, Gold = 2 }
        [SpoEntity("{{EntityId}}")]
        public partial class Customer
        {
            [SpoField(7)] public string Name { get; set; } = "";
            [SpoField(2)] public int? Age { get; set; }
            [SpoField(3)] public Tier Level { get; set; }
            [SpoField(4)] public byte[]? Photo { get; set; }
            [SpoField(5)] public System.DateTimeOffset Created { get; init; }
            public string NotPersistent { get; set; } = "";
        }
        public static class Probe { public static EntityMetadata Get() => Customer.SpoMetadata; }
        """;

    [Fact]
    public void ValidEntityGeneratesMetadataFieldByField()
    {
        var run = GeneratorHarness.Run(CustomerSource);
        Assert.Empty(run.GeneratorDiagnostics);
        Assert.Empty(run.CompilationErrors);

        var metadata = Probe(run);
        Assert.Equal(new Guid(EntityId), metadata.Id.Value);
        Assert.Equal("Shop.Model.Customer", metadata.ClrName);
        Assert.Equal([2, 3, 4, 5, 7], metadata.Properties.Select(p => p.Number));

        Check(metadata.Properties[0], 2, "Age", typeof(int), true);
        Check(metadata.Properties[1], 3, "Level", Type.GetType("Shop.Model.Tier") ?? metadata.Properties[1].ClrType, false);
        Assert.Equal("Tier", metadata.Properties[1].ClrType.Name);
        Assert.True(metadata.Properties[1].ClrType.IsEnum);
        Check(metadata.Properties[2], 4, "Photo", typeof(byte[]), true);
        Check(metadata.Properties[3], 5, "Created", typeof(DateTimeOffset), false);
        Check(metadata.Properties[4], 7, "Name", typeof(string), false);
    }

    [Fact]
    public void GeneratedCodeUsesNoReflection()
    {
        var run = GeneratorHarness.Run(CustomerSource);
        var generated = Assert.Single(run.GeneratedTrees).ToString();
        Assert.DoesNotContain("System.Reflection", generated);
        Assert.DoesNotContain("GetType(", generated);
        Assert.DoesNotContain("GetProperty", generated);
        Assert.DoesNotContain("Activator", generated);
    }

    [Fact]
    public void EveryDocumentedPropertyTypeIsSupported()
    {
        string[] types = ["bool", "int", "long", "decimal", "double", "string", "System.Guid", "System.DateTime", "System.DateTimeOffset", "byte[]", "System.DayOfWeek"];
        var members = types.Select((t, i) => $"[SpoField({i * 2 + 1})] public {t} P{i * 2} {{ get; set; }} = default!;\n[SpoField({i * 2 + 2})] public {t}? P{i * 2 + 1} {{ get; set; }}");
        var run = GeneratorHarness.Run($$"""
            using Sferix.SPO.Abstractions.Metadata;
            [SpoEntity("{{EntityId}}")] public partial class All { {{string.Join("\n", members)}} }
            """);
        Assert.Empty(run.GeneratorDiagnostics.Select(d => d.ToString()));
        Assert.Empty(run.CompilationErrors.Select(d => d.ToString()));
    }

    [Fact]
    public void NestedEntityInNamespaceGeneratesCompilableCode()
    {
        var run = GeneratorHarness.Run($$"""
            using Sferix.SPO.Abstractions.Metadata;
            namespace N { public partial class Outer { [SpoEntity("{{EntityId}}")] public partial class Inner { [SpoField(1)] public int X { get; set; } } } }
            """);
        Assert.Empty(run.GeneratorDiagnostics);
        Assert.Empty(run.CompilationErrors);
    }

    [Fact]
    public void EntityInGlobalNamespaceGeneratesCompilableCode()
    {
        var run = GeneratorHarness.Run($"[Sferix.SPO.Abstractions.Metadata.SpoEntity(\"{EntityId}\")] public partial class G {{ }}");
        Assert.Empty(run.GeneratorDiagnostics);
        Assert.Empty(run.CompilationErrors);
    }

    // The generated SpoMetadata is read by compiled code (Probe.Get); reflection only locates that entry point.
    [Fact]
    public void EnumPropertyCarriesItsUnderlyingType()
    {
        const string source = $$"""
            using Sferix.SPO.Abstractions.Metadata;
            namespace Shop.Model;
            public enum SmallE : byte { A }
            public enum DefaultE { A }
            public enum LargeE : long { A }
            [SpoEntity("{{EntityId}}")]
            public partial class Customer
            {
                [SpoField(1)] public SmallE A { get; set; }
                [SpoField(2)] public DefaultE B { get; set; }
                [SpoField(3)] public LargeE? C { get; set; }
                [SpoField(4)] public int D { get; set; }
            }
            public static class Probe { public static EntityMetadata Get() => Customer.SpoMetadata; }
            """;
        var run = GeneratorHarness.Run(source);
        Assert.Empty(run.GeneratorDiagnostics);
        var properties = Probe(run).Properties;
        Assert.Equal(typeof(byte), properties[0].EnumUnderlyingType);
        Assert.Equal(typeof(int), properties[1].EnumUnderlyingType);
        Assert.Equal(typeof(long), properties[2].EnumUnderlyingType);
        Assert.Null(properties[3].EnumUnderlyingType);
    }

    private static EntityMetadata Probe(GeneratorRun run)
    {
        using var stream = new MemoryStream();
        var emit = run.Output.Emit(stream);
        Assert.True(emit.Success, string.Join("\n", emit.Diagnostics));
        var assembly = Assembly.Load(stream.ToArray());
        var get = assembly.GetType("Shop.Model.Probe")!.GetMethod("Get")!.CreateDelegate<Func<EntityMetadata>>();
        return get();
    }

    private static void Check(PropertyMetadata actual, int number, string name, Type type, bool nullable)
    {
        Assert.Equal(number, actual.Number);
        Assert.Equal(name, actual.ClrName);
        Assert.Equal(type.Name, actual.ClrType.Name);
        Assert.Equal(nullable, actual.IsNullable);
    }
}
