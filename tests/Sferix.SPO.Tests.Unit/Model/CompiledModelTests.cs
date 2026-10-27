using System.Globalization;
using Sferix.SPO.Abstractions.Metadata;
using Sferix.SPO.Abstractions.Providers;
using Sferix.SPO.Core.Model;
using Xunit;

namespace Sferix.SPO.Tests.Unit.Model;

public sealed class CompiledModelTests
{
    private static readonly IReadOnlySet<SpoCapability> None = new HashSet<SpoCapability>();
    private static readonly IReadOnlySet<SpoCapability> Atomic = new HashSet<SpoCapability> { SpoCapability.AtomicSaveChanges };

    // A capability stub, not a database mock: Core only reads the declared set.
    private sealed class StubProvider(params SpoCapability[] capabilities) : ISpoProvider
    {
        public string Name => "Stub";
        public ISpoConnection Open(string connectionString) => throw new NotSupportedException();
        public IReadOnlySet<SpoCapability> Capabilities { get; } = new HashSet<SpoCapability>(capabilities);
    }

    private static EntityMetadata Entity(Guid id, string clrName = "App.Customer", string property = "Name") =>
        new(new EntityId(id), clrName, [new PropertyMetadata(1, property, typeof(string), false)]);

    [Fact]
    public void ValidModelActivates()
    {
        var model = CompiledModel.Build([Entity(Guid.NewGuid()), Entity(Guid.NewGuid(), "App.Order")], new StubProvider(SpoCapability.AtomicSaveChanges), Atomic);
        Assert.Equal(2, model.Entities.Count);
    }

    [Fact]
    public void MissingRequiredCapabilityFailsActivation()
    {
        var ex = Assert.Throws<SpoCapabilityMissingException>(() => CompiledModel.Build([Entity(Guid.NewGuid())], new StubProvider(), Atomic));
        Assert.Equal(SpoCapability.AtomicSaveChanges, ex.Capability);
        Assert.Equal("Stub", ex.ProviderName);
        Assert.Equal("SPO1001", ex.Code);
    }

    [Fact]
    public void CapabilityNotRequiredIsNotChecked() =>
        Assert.Single(CompiledModel.Build([Entity(Guid.NewGuid())], new StubProvider(), None).Entities);

    [Fact]
    public void DuplicateEntityIdFailsActivation()
    {
        var id = Guid.NewGuid();
        var ex = Assert.Throws<SpoModelException>(() => CompiledModel.Build([Entity(id), Entity(id, "App.Order")], new StubProvider(), None));
        Assert.Equal("SPO1002", ex.Code);
    }

    [Theory]
    [InlineData("App.Cust-omer", "Name")]
    [InlineData("App.1Customer", "Name")]
    [InlineData("App.Customer", "Na me")]
    [InlineData("App.Customer", "Name;DROP")]
    public void InvalidSqlIdentifierNameFailsActivation(string clrName, string property)
    {
        var ex = Assert.Throws<SpoModelException>(() => CompiledModel.Build([Entity(Guid.NewGuid(), clrName, property)], new StubProvider(), None));
        Assert.Equal("SPO1003", ex.Code);
    }

    [Fact]
    public void NestedEntityIsCheckedByItsOwnClassName() =>
        Assert.Single(CompiledModel.Build([Entity(Guid.NewGuid(), "App.Outer.Inner")], new StubProvider(), None).Entities);

    [Theory]
    [InlineData("de", "unterstützt")]
    [InlineData("hr", "ne podržava")]
    [InlineData("es", "no admite")]
    [InlineData("ja-JP", "does not support")]
    public void CapabilityMessageFollowsUiCulture(string culture, string expected)
    {
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo(culture);
            var ex = new SpoCapabilityMissingException(SpoCapability.AtomicSaveChanges, "Stub");
            Assert.Contains(expected, ex.Message);
            Assert.Contains("AtomicSaveChanges", ex.Message);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }
}
