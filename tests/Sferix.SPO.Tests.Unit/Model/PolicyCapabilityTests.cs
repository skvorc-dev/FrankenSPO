using Sferix.SPO.Abstractions.Metadata;
using Sferix.SPO.Abstractions.Providers;
using Sferix.SPO.Core.Model;
using Xunit;

namespace Sferix.SPO.Tests.Unit.Model;

public sealed class PolicyCapabilityTests
{
    private static readonly IReadOnlySet<SpoCapability> None = new HashSet<SpoCapability>();

    // A capability stub, not a database mock: Core only reads the declared set. It declares only what SQLite declares.
    private sealed class StubProvider : ISpoProvider
    {
        public string Name => "Stub";
        public IReadOnlySet<SpoCapability> Capabilities { get; } = new HashSet<SpoCapability> { SpoCapability.AtomicSaveChanges };
    }

    private static EntityMetadata Entity() =>
        new(new EntityId(Guid.NewGuid()), "App.Customer", [new PropertyMetadata(1, "Name", typeof(string), false)]);

    [Theory]
    [InlineData(SpoCapability.TenantPolicy)]
    [InlineData(SpoCapability.RowPolicy)]
    [InlineData(SpoCapability.FieldPolicy)]
    public void RequiredPolicyCapabilityFailsActivation(SpoCapability capability)
    {
        var required = new HashSet<SpoCapability> { capability };
        var ex = Assert.Throws<SpoCapabilityMissingException>(() => CompiledModel.Build([Entity()], new StubProvider(), required));
        Assert.Equal(capability, ex.Capability);
        Assert.Equal("SPO1001", ex.Code);
        Assert.Contains(capability.ToString(), ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ModelWithoutPolicyRequirementsActivates() =>
        Assert.Single(CompiledModel.Build([Entity()], new StubProvider(), None).Entities);
}
