using System.Linq.Expressions;
using Microsoft.Data.Sqlite;
using Sferix.SPO.Abstractions.Entities;
using Sferix.SPO.Abstractions.Metadata;
using Sferix.SPO.Abstractions.Modules;
using Sferix.SPO.Abstractions.ObjectSpace;
using Sferix.SPO.Core.Model;
using Sferix.SPO.Core.ObjectSpace;
using Sferix.SPO.Core.Query;
using Sferix.SPO.Provider.SQLite;
using Xunit;

namespace Sferix.SPO.Tests.SQLite;

[SpoEntity("5b0d7e42-91c3-4f6a-8d2e-7a41c9e03b55")]
public partial class Product : SPOBaseObject
{
    [SpoField(1)] public string Name { get; set; } = "";
    [SpoField(2)] public int Qty { get; set; }
    [SpoField(3)] public int? Rating { get; set; }
    [SpoField(4)] public bool Active { get; set; }
    [SpoField(5)] public string? Note { get; set; }
    [SpoField(6)] public Tier Level { get; set; }
    [SpoField(7)] public DateTime Stamp { get; set; }
    [SpoField(8)] public double Weight { get; set; }
    [SpoField(9)] public decimal Price { get; set; }
    [SpoField(10)] public DateTimeOffset Seen { get; set; }
}

public sealed class QueryModule : ISpoModule
{
    public IEnumerable<EntityMetadata> Entities => [Product.SpoMetadata];
}

public sealed class QueryTests : IDisposable
{
    private static readonly DateTime Base = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly string _path = Path.Combine(Path.GetTempPath(), $"spo-q-{Guid.NewGuid():N}.db");
    private readonly FakeTime _time = new(new DateTimeOffset(2026, 7, 1, 9, 0, 0, TimeSpan.Zero));

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        foreach (var file in new[] { _path, _path + "-wal", _path + "-shm" })
            File.Delete(file);
    }

    private static List<Product> Seed() =>
    [
        P("alpha", 1, 5, true, "a%b", Tier.Basic, 0, 1.5),
        P("beta", 2, null, false, "a_b", Tier.Gold, 1, 2.5),
        P("Alpha", 3, 3, true, "back\\slash", Tier.Basic, 2, 3.5),
        P("gamma", 4, 5, false, "it's", Tier.Gold, 3, 4.5),
        P("O'Brien", 5, null, true, "' OR 1=1 --", Tier.Basic, 4, 5.5),
        P("", 6, 1, false, null, Tier.Gold, 5, 6.5),
        P("delta", 7, 3, true, "100%", Tier.Basic, 6, 7.5),
    ];

    private static Product P(string name, int qty, int? rating, bool active, string? note, Tier level, int day, double weight) => new()
    {
        Name = name, Qty = qty, Rating = rating, Active = active, Note = note, Level = level,
        Stamp = Base.AddDays(day), Weight = weight, Price = qty, Seen = DateTimeOffset.UnixEpoch,
    };

    private ISpoObjectSpace Open() =>
        new SpoModelBuilder().AddModule<QueryModule>().UseTimeProvider(_time).Build(new SqliteProvider()).OpenObjectSpace($"Data Source={_path}");

    private ISpoObjectSpace OpenSeeded()
    {
        var space = Open();
        foreach (var product in Seed())
        {
            _time.Now = _time.Now.AddSeconds(1);
            space.Add(product);
        }
        space.SaveChanges();
        space.Dispose();
        return Open();
    }

    private static List<string> Names(IQueryable<Product> query) => [.. query.ToList().Select(p => p.Name)];

    public static TheoryData<Expression<Func<Product, bool>>> Predicates()
    {
        int min = 3;
        int? none = null;
        int? five = 5;
        var data = new TheoryData<Expression<Func<Product, bool>>>
        {
            x => x.Qty == 3, x => x.Qty != 3, x => x.Qty < 3, x => x.Qty <= 3, x => x.Qty > 3, x => x.Qty >= 3,
            x => 3 < x.Qty, x => x.Qty >= min, x => x.Qty > 2 && x.Qty < 6, x => x.Qty < 2 || x.Qty > 6, x => !(x.Qty == 3),
            x => x.Rating == 5, x => x.Rating != 5, x => x.Rating == null, x => x.Rating != null, x => x.Rating == none, x => x.Rating != none,
            x => x.Rating != five, x => !(x.Rating == five), x => x.Rating < 4, x => !(x.Rating < 4), x => x.Rating > 2,
            x => x.Rating.HasValue, x => !x.Rating.HasValue, x => x.Rating.HasValue && x.Rating.Value == 3,
            x => x.Active, x => !x.Active, x => x.Active == false, x => x.Active && x.Rating == 5,
            x => x.Level == Tier.Gold, x => x.Level != Tier.Gold, x => x.Level > Tier.Basic,
            x => x.Name == "alpha", x => x.Name != "alpha", x => x.Name == "", x => x.Name == "ALPHA",
            x => x.Note == null, x => x.Note != null, x => x.Note == "it's", x => x.Note != "it's",
            x => x.Stamp >= Base.AddDays(3), x => x.Stamp == Base.AddDays(2), x => x.Weight > 3.0,
            x => true, x => false,
            x => x.Name.StartsWith("a", StringComparison.Ordinal), x => x.Name.EndsWith("a"), x => x.Name.Contains("ph"),
            x => x.Name.StartsWith(""), x => x.Name.EndsWith(""), x => x.Name.Contains(""), x => !x.Name.Contains("a"),
            x => x.Name.Contains("ALPHA"), x => x.Name.EndsWith("longer than the value"),
        };
        return data;
    }

    [Theory]
    [MemberData(nameof(Predicates))]
    public void WhereMatchesLinqToObjects(Expression<Func<Product, bool>> predicate)
    {
        using var space = OpenSeeded();
        var expected = Seed().Where(predicate.Compile()).Select(p => p.Name).Order(StringComparer.Ordinal);
        var actual = space.Query<Product>().Where(predicate).ToList().Select(p => p.Name).Order(StringComparer.Ordinal);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ChainedWhereCombinesWithAnd()
    {
        using var space = OpenSeeded();
        Assert.Equal(["alpha", "delta"], Names(space.Query<Product>().Where(x => x.Active).Where(x => x.Rating != null).Where(x => x.Qty != 3).OrderBy(x => x.Qty)));
    }

    [Fact]
    public void ThenByKeepsOrderStable()
    {
        using var space = OpenSeeded();
        Assert.Equal(["alpha", "Alpha", "O'Brien", "delta", "beta", "gamma", ""],
            Names(space.Query<Product>().OrderByDescending(x => x.Active).ThenBy(x => x.Qty)));
        Assert.Equal(["delta", "O'Brien", "Alpha", "alpha", "", "gamma", "beta"],
            Names(space.Query<Product>().OrderByDescending(x => x.Active).ThenByDescending(x => x.Qty)));
    }

    [Fact]
    public void OrderByAndThenByUseColumns()
    {
        using var space = OpenSeeded();
        Assert.Equal(["O'Brien", "beta", "", "delta", "Alpha", "gamma", "alpha"],
            Names(space.Query<Product>().OrderBy(x => x.Rating).ThenByDescending(x => x.Qty)));
        Assert.Equal(["Alpha", "alpha", "beta"], Names(space.Query<Product>().Where(x => x.Qty < 4).OrderBy(x => x.Name)));
        Assert.Equal(["", "Alpha", "O'Brien", "alpha", "beta", "delta", "gamma"], Names(space.Query<Product>().OrderBy(x => x.Name)));
        Assert.Equal(["gamma", "delta", "beta", "alpha", "O'Brien", "Alpha", ""], Names(space.Query<Product>().OrderByDescending(x => x.Name)));
    }

    [Fact]
    public void OrderByIdIsInsertionOrderForUuidV7()
    {
        using var space = OpenSeeded();
        Assert.Equal(Seed().Select(p => p.Name), Names(space.Query<Product>().OrderBy(x => x.Id)));
        Assert.Equal(Seed().Select(p => p.Name).Reverse(), Names(space.Query<Product>().OrderByDescending(x => x.Id)));
        Assert.Equal(Seed().Select(p => p.Name), Names(space.Query<Product>())); // default: by key
    }

    [Fact]
    public void SkipAndTakePage()
    {
        using var space = OpenSeeded();
        var q = space.Query<Product>().OrderBy(x => x.Qty);
        Assert.Equal(["beta", "Alpha"], Names(q.Skip(1).Take(2)));
        Assert.Equal(["delta"], Names(q.Skip(6)));
        Assert.Equal(["alpha", "beta", "Alpha"], Names(q.Take(3)));
        Assert.Empty(q.Take(0).ToList());
        Assert.Empty(q.Skip(7).ToList());
        Assert.Equal(["Alpha", "gamma"], Names(q.Skip(1).Skip(1).Take(5).Take(2)));
        var skipped = 2;
        Assert.Equal(5, q.Skip(skipped).ToList().Count);
        Assert.Equal(7, q.Skip(-3).ToList().Count);
        Assert.Empty(q.Take(-1).ToList());
    }

    [Fact]
    public void StringsWithSpecialCharactersMatchLiterally()
    {
        using var space = OpenSeeded();
        Assert.Equal(["alpha"], Names(space.Query<Product>().Where(x => x.Note!.StartsWith("a%"))));
        Assert.Equal(["alpha", "delta"], Names(space.Query<Product>().Where(x => x.Note!.Contains("%"))));
        Assert.Equal(["beta"], Names(space.Query<Product>().Where(x => x.Note!.Contains("_"))));
        Assert.Equal(["Alpha"], Names(space.Query<Product>().Where(x => x.Note!.Contains("\\"))));
        Assert.Equal(["gamma"], Names(space.Query<Product>().Where(x => x.Note!.EndsWith("'s"))));
        Assert.Equal(["O'Brien"], Names(space.Query<Product>().Where(x => x.Note == "' OR 1=1 --")));
        Assert.Equal(["O'Brien"], Names(space.Query<Product>().Where(x => x.Note!.StartsWith("' OR 1=1"))));
        Assert.Equal(["O'Brien"], Names(space.Query<Product>().Where(x => x.Name == "O'Brien")));
        var hostile = "'; DROP TABLE Product; --";
        Assert.Empty(space.Query<Product>().Where(x => x.Name == hostile).ToList());
        Assert.Empty(space.Query<Product>().Where(x => x.Name.Contains(hostile)).ToList());
        Assert.Empty(space.Query<Product>().Where(x => x.Name.StartsWith("%")).ToList());
        Assert.Empty(space.Query<Product>().Where(x => x.Name.StartsWith("_lpha")).ToList());
        Assert.Equal(7, space.Query<Product>().ToList().Count);
    }

    [Fact]
    public void MatchingIsCaseSensitiveAndEmptyTextMatchesNonNullValues()
    {
        using var space = OpenSeeded();
        Assert.Equal(["alpha"], Names(space.Query<Product>().Where(x => x.Name.StartsWith("alp"))));
        Assert.Equal(["Alpha"], Names(space.Query<Product>().Where(x => x.Name.StartsWith("Alp"))));
        Assert.Equal(7, space.Query<Product>().Where(x => x.Name.StartsWith("")).ToList().Count);
        Assert.Equal(7, space.Query<Product>().Where(x => x.Name.EndsWith("")).ToList().Count);
        Assert.Equal(7, space.Query<Product>().Where(x => x.Name.Contains("")).ToList().Count);
        var empty = "";
        Assert.Equal(6, space.Query<Product>().Where(x => x.Note!.Contains(empty)).ToList().Count); // the null note never matches
    }

    [Fact]
    public void NullCapturedValueFollowsCSharpSemantics()
    {
        using var space = OpenSeeded();
        int? none = null;
        Assert.Equal(["beta", "O'Brien"], Names(space.Query<Product>().Where(x => x.Rating == none).OrderBy(x => x.Qty)));
        Assert.Equal(5, space.Query<Product>().Where(x => x.Rating != none).ToList().Count);
        Assert.Empty(space.Query<Product>().Where(x => x.Rating > none).ToList());
    }

    [Fact]
    public void NotEqualOnNullableColumnIncludesNullRows()
    {
        using var space = OpenSeeded();
        Assert.Equal(["beta", "Alpha", "O'Brien", "", "delta"], Names(space.Query<Product>().Where(x => x.Rating != 5).OrderBy(x => x.Qty)));
    }

    [Fact]
    public void SoftDeletedRowsAreHiddenUnlessIncluded()
    {
        using (var space = OpenSeeded())
        {
            var alpha = space.Query<Product>().Where(x => x.Name == "alpha").ToList().Single();
            space.Delete(alpha);
            space.SaveChanges();
        }

        using var check = Open();
        Assert.Equal(6, check.Query<Product>().ToList().Count);
        Assert.Equal(7, check.Query<Product>().IncludeDeleted().ToList().Count);
        Assert.Equal(["alpha"], Names(check.Query<Product>().IncludeDeleted().Where(x => x.IsDeleted)));
        Assert.Equal(["alpha"], Names(check.Query<Product>().Where(x => x.IsDeleted).IncludeDeleted()));
        Assert.Empty(check.Query<Product>().Where(x => x.IsDeleted).ToList());
    }

    [Fact]
    public void PendingDeleteIsLeftOutAndAddedEntitiesAreNotResults()
    {
        using var space = OpenSeeded();
        var beta = space.Query<Product>().Where(x => x.Name == "beta").ToList().Single();
        space.Delete(beta);
        space.Add(new Product { Name = "zeta", Seen = DateTimeOffset.UnixEpoch });
        var names = Names(space.Query<Product>().IncludeDeleted());
        Assert.DoesNotContain("beta", names);
        Assert.DoesNotContain("zeta", names);
        Assert.Equal(6, names.Count);
    }

    [Fact]
    public void TrackedInstanceWinsWithItsInMemoryValues()
    {
        using var space = OpenSeeded();
        var first = space.Query<Product>().Where(x => x.Name == "beta").ToList().Single();
        Assert.Same(first, space.Get<Product>(first.Id));
        first.Qty = 99;
        first.Name = "renamed";
        var again = space.Query<Product>().Where(x => x.Qty == 2).ToList().Single(); // matches the stored value
        Assert.Same(first, again);
        Assert.Equal("renamed", again.Name);
        Assert.Empty(space.Query<Product>().Where(x => x.Qty == 99).ToList()); // the filter runs on stored values
    }

    [Fact]
    public async Task ToListAsyncMatchesToList()
    {
        await using var space = OpenSeeded();
        var query = space.Query<Product>().Where(x => x.Active).OrderBy(x => x.Qty);
        Assert.Equal(Names(query), (await query.ToListAsync(CancellationToken.None)).Select(p => p.Name));
        using var cancelled = new CancellationTokenSource();
        await cancelled.CancelAsync();
        await Assert.ThrowsAsync<OperationCanceledException>(() => query.ToListAsync(cancelled.Token));
        Assert.Equal(4, (await query.IncludeDeleted().ToListAsync(CancellationToken.None)).Count);
    }

    [Fact]
    public void UnsupportedCasesFailWithSpo1013()
    {
        using var space = OpenSeeded();
        var q = space.Query<Product>();
        var cases = new (string Name, Func<object> Run)[]
        {
            ("decimal in Where", () => q.Where(x => x.Price > 2m).ToList()),
            ("decimal equality", () => q.Where(x => x.Price == 2m).ToList()),
            ("DateTimeOffset in Where", () => q.Where(x => x.Seen == DateTimeOffset.UnixEpoch).ToList()),
            ("decimal in OrderBy", () => q.OrderBy(x => x.Price).ToList()),
            ("DateTimeOffset in OrderBy", () => q.OrderByDescending(x => x.Seen).ToList()),
            ("ToUpper", () => q.Where(x => x.Name.ToUpper() == "ALPHA").ToList()),
            ("culture-insensitive comparison", () => q.Where(x => x.Name.StartsWith("a", StringComparison.OrdinalIgnoreCase)).ToList()),
            ("column comparison", () => q.Where(x => x.Qty == x.Rating).ToList()),
            ("arithmetic", () => q.Where(x => x.Qty + 1 == 3).ToList()),
            ("Select", () => q.Select(x => x.Name).ToList()),
            ("GroupBy", () => q.GroupBy(x => x.Active).ToList()),
            ("Count", () => q.Count()),
            ("First", () => q.First()),
            ("Any", () => q.Any()),
            ("Where after Take", () => q.Take(2).Where(x => x.Active).ToList()),
            ("OrderBy after Skip", () => q.Skip(1).OrderBy(x => x.Qty).ToList()),
            ("Skip after Take", () => q.Take(3).Skip(1).ToList()),
            ("Distinct", () => q.Distinct().ToList()),
            ("method on column", () => q.Where(x => x.Name.Length == 3).ToList()),
        };
        foreach (var (name, run) in cases)
        {
            var ex = Assert.Throws<SpoQueryNotSupportedException>(run);
            Assert.True(ex.Code == "SPO1013", name);
            Assert.False(string.IsNullOrEmpty(ex.Operator), name);
        }
    }

    [Fact]
    public void RejectedColumnMessageNamesColumnButNoValues()
    {
        using var space = OpenSeeded();
        var secret = 12345.678m;
        var ex = Assert.Throws<SpoQueryNotSupportedException>(() => space.Query<Product>().Where(x => x.Price == secret).ToList());
        Assert.Contains("Price", ex.Message);
        Assert.DoesNotContain("12345", ex.Message);
        Assert.DoesNotContain("SELECT", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MessagesAreLocalized()
    {
        using var space = OpenSeeded();
        var previous = System.Globalization.CultureInfo.CurrentUICulture;
        try
        {
            System.Globalization.CultureInfo.CurrentUICulture = new System.Globalization.CultureInfo("de");
            var ex = Assert.Throws<SpoQueryNotSupportedException>(() => space.Query<Product>().Where(x => x.Price > 1m).ToList());
            Assert.Contains("Spalte", ex.Message);
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentUICulture = previous;
        }
    }

    [Fact]
    public void QueryOfATypeOutsideTheModelFailsAndUseAfterDisposeThrows()
    {
        var space = OpenSeeded();
        Assert.Equal("SPO1008", Assert.Throws<SpoObjectSpaceException>(() => space.Query<Person>()).Code);
        space.Dispose();
        Assert.Throws<ObjectDisposedException>(() => space.Query<Product>());
    }

    [Fact]
    public void QueryingWhileAnotherCallRunsIsReportedAsConcurrentUse()
    {
        using var space = OpenSeeded();
        var gadgetSpace = space;
        var ex = Assert.Throws<SpoObjectSpaceException>(() =>
            gadgetSpace.Query<Product>().Where(x => x.Name == Probe(gadgetSpace)).ToList());
        Assert.Equal("SPO1010", ex.Code);
    }

    private static string Probe(ISpoObjectSpace space)
    {
        // Evaluated by the translator while the ObjectSpace is busy executing this very query.
        space.Query<Product>().ToList();
        return "x";
    }
}
