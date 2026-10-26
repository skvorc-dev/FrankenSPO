using Sferix.SPO.Abstractions.Metadata;
using Sferix.SPO.Core.Sql;

namespace Sferix.SPO.Tests.Unit;

public sealed class IdentifierTests
{
    [Theory]
    [InlineData("Customer")]
    [InlineData("_tenant_01")]
    [InlineData("A1")]
    public void AcceptsConservativeIdentifiers(string input)
        => Assert.Equal(input, new SqlIdentifier(input).Value);

    [Theory]
    [InlineData("Customer;DROP TABLE Customer")]
    [InlineData("Customer -- comment")]
    [InlineData("Customer.Name")]
    [InlineData("x OR 1=1")]
    [InlineData(" customer")]
    [InlineData("1customer")]
    [InlineData("name\"quoted")]
    public void RejectsUnsafeIdentifiers(string input)
        => Assert.Throws<ArgumentException>(() => new SqlIdentifier(input));

    [Fact]
    public void RejectsEmptyEntityId()
        => Assert.Throws<ArgumentException>(() => new EntityId(Guid.Empty));

    [Fact]
    public void PreservesEntityId()
    {
        var id = Guid.NewGuid();
        Assert.Equal(id, new EntityId(id).Value);
    }
}
