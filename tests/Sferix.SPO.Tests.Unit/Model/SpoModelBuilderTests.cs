using Sferix.SPO.Abstractions.Metadata;
using Sferix.SPO.Abstractions.Modules;
using Sferix.SPO.Abstractions.Providers;
using Sferix.SPO.Core.Model;
using Xunit;

namespace Sferix.SPO.Tests.Unit.Model;

public sealed class SpoModelBuilderTests
{
    private static readonly Guid CustomerId = new("0195f0a2-0000-7000-8000-000000000001");
    private static readonly Guid OrderId = new("0195f0a2-0000-7000-8000-000000000002");

    // A capability stub, not a database mock: Core only reads the declared set.
    private sealed class StubProvider(params SpoCapability[] capabilities) : ISpoProvider
    {
        public string Name => "Stub";
        public IReadOnlySet<SpoCapability> Capabilities { get; } = new HashSet<SpoCapability>(capabilities);
    }

    private sealed class CustomerModule : ISpoModule
    {
        public IEnumerable<EntityMetadata> Entities =>
            [new EntityMetadata(new EntityId(CustomerId), "App.Customer", [new PropertyMetadata(1, "Name", typeof(string), false)])];
    }

    private sealed class OrderModule : ISpoModule
    {
        public IEnumerable<EntityMetadata> Entities =>
            [new EntityMetadata(new EntityId(OrderId), "App.Order", [new PropertyMetadata(1, "Total", typeof(decimal), false)])];
    }

    // Contributes the same entity ID as CustomerModule under another name: a conflict across modules.
    private sealed class LegacyCustomerModule : ISpoModule
    {
        public IEnumerable<EntityMetadata> Entities =>
            [new EntityMetadata(new EntityId(CustomerId), "Legacy.Customer", [new PropertyMetadata(1, "Name", typeof(string), false)])];
    }

    private static readonly StubProvider Atomic = new(SpoCapability.AtomicSaveChanges);

    [Fact]
    public void TwoModulesActivateInRegistrationOrder()
    {
        var model = new SpoModelBuilder().AddModule<CustomerModule>().AddModule<OrderModule>().Build(Atomic);
        Assert.Equal(["App.Customer", "App.Order"], model.Entities.Select(entity => entity.ClrName));
    }

    [Fact]
    public void ModuleAddedTwiceIsRegisteredOnce()
    {
        var model = new SpoModelBuilder().AddModule<CustomerModule>().AddModule<CustomerModule>().Build(Atomic);
        Assert.Single(model.Entities);
    }

    [Fact]
    public void DuplicateEntityAcrossModulesFailsWithSpo1002()
    {
        var builder = new SpoModelBuilder().AddModule<CustomerModule>().AddModule<LegacyCustomerModule>();
        var ex = Assert.Throws<SpoModelException>(() => builder.Build(Atomic));
        Assert.Equal("SPO1002", ex.Code);
    }

    [Fact]
    public void MissingRequiredCapabilityFailsWithSpo1001()
    {
        var builder = new SpoModelBuilder().AddModule<CustomerModule>();
        var ex = Assert.Throws<SpoCapabilityMissingException>(() => builder.Build(new StubProvider()));
        Assert.Equal("SPO1001", ex.Code);
        Assert.Equal(SpoCapability.AtomicSaveChanges, ex.Capability);
    }

    [Fact]
    public void NullProviderIsRejected() =>
        Assert.Throws<ArgumentNullException>(() => new SpoModelBuilder().Build(null!));
}
