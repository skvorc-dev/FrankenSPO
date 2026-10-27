using Sferix.SPO.Core.ObjectSpace;
using Xunit;

namespace Sferix.SPO.Tests.Unit.ObjectSpace;

public sealed class ComparerTests
{
    private static bool Same(Type type, object? a, object? b) => EntityInfo.ComparerFor(type)(a, b);

    [Fact]
    public void DecimalComparesValueAndScale()
    {
        Assert.True(Same(typeof(decimal), 1.50m, 1.50m));
        Assert.False(Same(typeof(decimal), 1.5m, 1.50m));
        Assert.False(Same(typeof(decimal), 1.5m, 1.6m));
    }

    [Fact]
    public void DateTimeComparesTicksAndKind()
    {
        var utc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        Assert.True(Same(typeof(DateTime), utc, new DateTime(utc.Ticks, DateTimeKind.Utc)));
        Assert.False(Same(typeof(DateTime), utc, new DateTime(utc.Ticks, DateTimeKind.Local)));
        Assert.False(Same(typeof(DateTime), utc, utc.AddTicks(1)));
    }

    [Fact]
    public void DateTimeOffsetComparesTicksAndOffset()
    {
        var a = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.FromHours(2));
        Assert.True(Same(typeof(DateTimeOffset), a, new DateTimeOffset(a.Ticks, TimeSpan.FromHours(2))));
        Assert.False(Same(typeof(DateTimeOffset), a, a.ToUniversalTime())); // same instant, other offset
        Assert.False(Same(typeof(DateTimeOffset), a, a.AddTicks(1)));
    }

    [Fact]
    public void ByteArrayComparesContent()
    {
        Assert.True(Same(typeof(byte[]), new byte[] { 1, 2 }, new byte[] { 1, 2 }));
        Assert.False(Same(typeof(byte[]), new byte[] { 1, 2 }, new byte[] { 1, 3 }));
        Assert.False(Same(typeof(byte[]), new byte[] { 1 }, new byte[] { 1, 2 }));
    }

    [Fact]
    public void StringComparesOrdinal()
    {
        Assert.True(Same(typeof(string), "a", "a"));
        Assert.False(Same(typeof(string), "a", "A"));
        Assert.False(Same(typeof(string), "é", "é")); // canonically equal, not ordinal-equal
    }

    [Fact]
    public void OtherTypesUseEquals()
    {
        Assert.True(Same(typeof(int), 5, 5));
        Assert.False(Same(typeof(int), 5, 6));
        Assert.True(Same(typeof(Guid), Guid.Empty, Guid.Empty));
        Assert.False(Same(typeof(bool), true, false));
    }

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(decimal))]
    [InlineData(typeof(byte[]))]
    [InlineData(typeof(int))]
    public void NullEqualsOnlyNull(Type type)
    {
        Assert.True(Same(type, null, null));
        Assert.False(Same(type, null, type == typeof(string) ? "x" : type == typeof(byte[]) ? new byte[0] : type == typeof(int) ? 0 : 0m));
    }
}
