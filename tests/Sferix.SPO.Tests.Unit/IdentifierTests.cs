using Xunit;
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
    public void AcceptsIdentifierOf63Characters()
    {
        var input = new string('a', 63);
        Assert.Equal(input, new SqlIdentifier(input).Value);
    }

    [Fact]
    public void RejectsIdentifierOf64Characters()
        => Assert.Throws<ArgumentException>(() => new SqlIdentifier(new string('a', 64)));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    // The null-forgiving operator is deliberate: the test passes null on purpose to pin the guard clause.
    public void RejectsNullEmptyOrWhitespaceIdentifiers(string? input)
        => Assert.ThrowsAny<ArgumentException>(() => new SqlIdentifier(input!));

    [Fact]
    public void RejectsNonAsciiLetterIdentifiers()
        => Assert.Throws<ArgumentException>(() => new SqlIdentifier("KupacČ"));

    [Fact]
    public void FormatsEntityIdAsDFormat()
    {
        var id = Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3301");
        Assert.Equal("3f2504e0-4f89-11d3-9a0c-0305e82c3301", new EntityId(id).ToString());
    }

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
